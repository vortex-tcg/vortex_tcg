using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using VortexTCG.Scripts.DTOs;

namespace VortexTCG.Scripts.Features.Collection.UI
{
    public partial class CollectionUI
    {
        private const int MaxDeckCards = 30;

        private void DisplayDecks(List<UserCollectionDeckDto> decks)
        {
            if (deckButtonsContainer == null) return;

            HashSet<Guid> deckIds = decks != null
                ? new HashSet<Guid>(decks.Where(deck => deck != null).Select(deck => deck.DeckId))
                : new HashSet<Guid>();

            Dictionary<Guid, Button> existingButtons = deckButtonsContainer
                .Children()
                .OfType<Button>()
                .Where(button => button.userData is Guid)
                .ToDictionary(button => (Guid)button.userData, button => button);

            foreach (KeyValuePair<Guid, Button> pair in existingButtons)
            {
                if (!deckIds.Contains(pair.Key))
                    pair.Value.RemoveFromHierarchy();
            }

            existingButtons = deckButtonsContainer
                .Children()
                .OfType<Button>()
                .Where(button => button.userData is Guid)
                .ToDictionary(button => (Guid)button.userData, button => button);

            bool hasExistingButtons = existingButtons.Count > 0;
            selectedDeckButton = hasExistingButtons ? selectedDeckButton : null;

            if (decks == null || decks.Count == 0)
            {
                if (!hasExistingButtons)
                    ResetCurrentDeckState();

                return;
            }

            Button firstCreatedButton = null;
            UserCollectionDeckDto firstDeck = null;

            foreach (UserCollectionDeckDto deck in decks)
            {
                if (deck == null) continue;

                if (existingButtons.TryGetValue(deck.DeckId, out Button existingButton))
                {
                    existingButton.text = string.IsNullOrWhiteSpace(deck.DeckName) ? "Deck" : deck.DeckName;
                    if (firstCreatedButton == null)
                    {
                        firstCreatedButton = existingButton;
                        firstDeck = deck;
                    }

                    continue;
                }

                Button btn = new Button();
                btn.userData = deck.DeckId;
                btn.name = $"DeckButton_{deck.DeckId}";
                btn.text = string.IsNullOrWhiteSpace(deck.DeckName) ? "Deck" : deck.DeckName;
                btn.AddToClassList("vortexButton");
                btn.AddToClassList("interactive");
                btn.style.borderTopWidth = 0;
                btn.style.borderRightWidth = 0;
                btn.style.borderBottomWidth = 0;
                btn.style.borderLeftWidth = 0;
                btn.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0f));

                var capturedId = deck.DeckId;
                btn.clicked += () => SelectDeckByButton(capturedId, btn);
                deckButtonsContainer.Add(btn);

                if (firstCreatedButton == null)
                {
                    firstCreatedButton = btn;
                    firstDeck = deck;
                }
            }

            bool selectedButtonStillExists = selectedDeckButton != null && selectedDeckButton.parent != null;

            if (firstDeck != null && firstCreatedButton != null && !selectedButtonStillExists)
            {
                SelectDeck(firstDeck, firstCreatedButton);
            }
        }

        private void InitializeCreateDeckModal()
        {
            if (addDeckButton != null)
            {
                addDeckButton.clicked += OpenCreateDeckModal;
            }

            if (createModalHUD != null)
                createModalHUD.style.display = DisplayStyle.None;

            if (createModalConfirmButton != null)
                createModalConfirmButton.clicked += ConfirmCreateDeck;

            if (createModalCancelButton != null)
                createModalCancelButton.clicked += CloseCreateDeckModal;

            if (createDeckInput != null)
            {
                createDeckInput.isDelayed = false;
                createDeckInput.RegisterCallback<KeyDownEvent>(OnCreateDeckInputKeyDown);
            }
        }

        private void OpenCreateDeckModal()
        {
            if (createModalHUD == null)
                return;

            if (currentChampionId == Guid.Empty || currentFactionId == Guid.Empty)
            {
                Debug.LogError("[DeckUI] Impossible de creer un deck sans champion/faction selectionnes");
                return;
            }

            if (createDeckInput != null)
                createDeckInput.value = string.Empty;

            createModalHUD.style.display = DisplayStyle.Flex;

            if (createDeckInput != null)
            {
                createDeckInput.schedule.Execute(() =>
                {
                    createDeckInput.Focus();
                    createDeckInput.SelectAll();
                });
            }
        }

        private void CloseCreateDeckModal()
        {
            if (createModalHUD != null)
                createModalHUD.style.display = DisplayStyle.None;
        }

        private void OnCreateDeckInputKeyDown(KeyDownEvent evt)
        {
            if (evt == null)
                return;

            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                evt.StopPropagation();
                ConfirmCreateDeck();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                evt.StopPropagation();
                CloseCreateDeckModal();
            }
        }

        private void ConfirmCreateDeck()
        {
            if (currentChampionId == Guid.Empty || currentFactionId == Guid.Empty)
                return;

            CloseCreateDeckModal();
            StartCoroutine(CreateNewDeck());
        }

        private IEnumerator CreateNewDeck()
        {
            if (deckService == null)
                deckService = new VortexTCG.Scripts.Features.Deck.Services.DeckService();

            string deckName = createDeckInput != null ? createDeckInput.value?.Trim() : string.Empty;
            string error = null;

            yield return deckService.CreateDeckAsync(
                deckName,
                currentChampionId,
                currentFactionId,
                onSuccess: () => Debug.Log($"[DeckUI] CreateDeck succeeded Name='{deckName}'"),
                onError: serviceError => error = serviceError
            );

            if (!string.IsNullOrWhiteSpace(error))
            {
                Debug.LogError(error);
                yield break;
            }

            yield return LoadUserCollectionAndDecks();
        }

        private void SelectDeck(UserCollectionDeckDto deck, Button selectedButton)
        {
            CommitDeckNameEditIfNeeded();
            HighlightSelectedDeck(selectedButton);
            selectedDeckButton = selectedButton;
            currentChampionId = deck.ChampionId;
            currentFactionId = deck.FactionId;
            Debug.Log($"[DeckUI] SelectDeck deckId={deck.DeckId} deckName='{deck.DeckName}'");
            SetCurrentDeckName(deck.DeckName);
            StartCoroutine(LoadAndShowDeck(deck.DeckId, deck.DeckName));
        }

        private void SelectDeckByButton(Guid deckId, Button selectedButton)
        {
            CommitDeckNameEditIfNeeded();
            HighlightSelectedDeck(selectedButton);
            selectedDeckButton = selectedButton;
            currentChampionId = Guid.Empty;
            currentFactionId = Guid.Empty;
            string deckName = selectedButton != null ? selectedButton.text : "Deck";
            Debug.Log($"[DeckUI] SelectDeckByButton deckId={deckId} deckName='{deckName}'");
            SetCurrentDeckName(deckName);
            StartCoroutine(LoadAndShowDeck(deckId, deckName));
        }

        private void HighlightSelectedDeck(Button selectedButton)
        {
            if (deckButtonsContainer == null) return;

            foreach (VisualElement child in deckButtonsContainer.Children())
            {
                if (child is Button button)
                    button.RemoveFromClassList("deck-button-selected");
            }

            if (selectedButton != null)
                selectedButton.AddToClassList("deck-button-selected");
        }

        private void ClearSelectedDeckCards()
        {
            if (selectedDeckCardsContainer != null)
                selectedDeckCardsContainer.Clear();
        }

        private void ResetCurrentDeckState()
        {
            currentDeckId = Guid.Empty;
            SetCurrentDeckName("");
            currentChampionId = Guid.Empty;
            currentFactionId = Guid.Empty;
            currentDeckCards.Clear();
            ClearSelectedDeckCards();
            RefreshDeckLengthLabel();
            selectedDeckButton = null;
            SetDeckNameEditMode(false);
            CloseDeleteDeckModal();
            RefreshDeleteDeckButtonState();
        }

        private IEnumerator LoadAndShowDeck(Guid deckId, string deckName)
        {
            if (deckService == null)
                deckService = new VortexTCG.Scripts.Features.Deck.Services.DeckService();

            currentDeckId = deckId;
            Debug.Log($"[DeckUI] LoadAndShowDeck deckId={deckId} deckName='{deckName}'");
            SetCurrentDeckName(deckName);

            DeckDataDto deckData = null;
            string error = null;

            yield return deckService.FetchDeckData(deckId, d => deckData = d, e => error = e);

            if (!string.IsNullOrWhiteSpace(error))
            {
                Debug.LogError(error);
                yield break;
            }

            currentChampionId = deckData?.Champion?.ChampionID ?? Guid.Empty;
            currentFactionId = deckData?.Champion?.FactionId ?? Guid.Empty;
            currentDeckCards = NormalizeDeckCards(deckData?.Cards);

            ShowSelectedDeckCards(currentDeckCards);
            RefreshDeckLengthLabel();
            RefreshDeleteDeckButtonState();
        }

        private void ShowSelectedDeckCards(List<DeckCardDto> cards)
        {
            if (selectedDeckCardsContainer == null)
                return;

            selectedDeckCardsContainer.Clear();

            foreach (DeckCardDto c in cards)
            {
                if (c == null) continue;

                for (int i = 0; i < Math.Max(c.Quantity, 1); i++)
                {
                    VisualElement cardElement = cardTemplate.CloneTree();
                    CardDto previewCard = CreateDeckPreviewCard(c);

                    BindCardVisual(cardElement, previewCard);
                    cardElement.RegisterCallback<MouseEnterEvent>(_ => ShowCardPreview(previewCard));
                    cardElement.RegisterCallback<MouseLeaveEvent>(_ => HideCardPreview());
                    RegisterDeckCardDrag(cardElement, c, previewCard);

                    selectedDeckCardsContainer.Add(cardElement);
                }
            }

            RefreshDeckLengthLabel();
        }

        private void AddCardToSelectedDeck(UserCollectionCardDto cardData)
        {
            if (cardData?.Card == null || cardData.CollectionCardId == Guid.Empty || currentDeckId == Guid.Empty)
                return;

            if (GetCurrentDeckCardCount() >= MaxDeckCards)
            {
                Debug.Log("[DeckUI] Limite de 30 cartes atteinte pour ce deck.");
                RefreshDeckLengthLabel();
                return;
            }

            DeckCardDto existing = currentDeckCards.Find(card => card.CollectionCardId == cardData.CollectionCardId);
            if (existing != null)
            {
                existing.Quantity += 1;
            }
            else
            {
                currentDeckCards.Add(new DeckCardDto
                {
                    CollectionCardId = cardData.CollectionCardId,
                    CardId = cardData.Card.Id,
                    Name = cardData.Card.Name,
                    Hp = cardData.Card.Hp,
                    Attack = cardData.Card.Attack,
                    Cost = cardData.Card.Cost,
                    Description = cardData.Card.Description,
                    Picture = cardData.Card.Picture,
                    Extension = cardData.Card.Extension,
                    CardType = cardData.Card.CardType,
                    Price = cardData.Card.Price,
                    Classes = cardData.Card.Class ?? new List<string>(),
                    Quantity = 1,
                    Rarity = cardData.OwnData != null && cardData.OwnData.Count > 0 ? cardData.OwnData[0].Rarity : ""
                });
            }

            ShowSelectedDeckCards(currentDeckCards);
            PersistDeckChanges();
        }

        private void RemoveCardFromSelectedDeck(DeckCardDto deckCard)
        {
            if (deckCard == null || deckCard.CollectionCardId == Guid.Empty || currentDeckId == Guid.Empty)
                return;

            DeckCardDto existing = currentDeckCards.Find(card => card.CollectionCardId == deckCard.CollectionCardId);
            if (existing == null)
                return;

            if (existing.Quantity > 1)
                existing.Quantity -= 1;
            else
                currentDeckCards.Remove(existing);

            ShowSelectedDeckCards(currentDeckCards);
            PersistDeckChanges();
        }

        private int GetCurrentDeckCardCount()
        {
            if (currentDeckCards == null || currentDeckCards.Count == 0)
                return 0;

            return currentDeckCards.Sum(card => Math.Max(card?.Quantity ?? 0, 1));
        }

        private void RefreshDeckLengthLabel()
        {
            if (deckLengthLabel == null)
                return;

            int count = GetCurrentDeckCardCount();
            deckLengthLabel.text = $"({count}/{MaxDeckCards})";
        }

        private void PersistDeckChanges()
        {
            if (deckService == null || currentDeckId == Guid.Empty)
                return;

            // Protect payload against duplicated card rows accumulated client-side.
            currentDeckCards = NormalizeDeckCards(currentDeckCards);

            UpdateDeckDto payload = new UpdateDeckDto
            {
                Name = currentDeckName,
                ChampionId = currentChampionId,
                FactionId = currentFactionId,
                Cards = currentDeckCards.Select(card => new UpdateDeckCardDto
                {
                    CollectionCardId = card.CollectionCardId,
                    Quantity = card.Quantity
                }).ToList()
            };

            Debug.Log($"[DeckUI] PersistDeckChanges sending deckId={currentDeckId} Name='{payload.Name}' Cards={payload.Cards.Count}");
            StartCoroutine(deckService.UpdateDeckAsync(
                currentDeckId,
                payload,
                onSuccess: () => Debug.Log("[DeckUI] Persist succeeded"),
                onError: error => Debug.LogError(error)
            ));
        }

        private void InitializeDeckNameEditor()
        {
            if (deckNameContainer == null || deckNameLabel == null || editDeckNameButton == null)
                return;

            deckNameContainer.style.display = DisplayStyle.Flex;
            deckNameContainer.style.alignItems = Align.Center;

            // Keep the label compact so the edit button stays next to it
            deckNameLabel.style.flexGrow = 0f;
            deckNameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;

            editDeckNameButton.style.flexShrink = 0f;
            editDeckNameButton.text = string.Empty;
            editDeckNameButton.tooltip = "Edit deck name";
            editDeckNameButton.style.marginLeft = 6;

            deckNameTextField = rootVisualElement.Q<TextField>("DeckNameInput");
            if (deckNameTextField != null)
            {
                deckNameTextField.isDelayed = false;
                deckNameTextField.style.display = DisplayStyle.None;
                deckNameTextField.style.flexGrow = 1f;
                deckNameTextField.style.marginRight = 8;
                deckNameTextField.RegisterCallback<KeyDownEvent>(OnDeckNameInputKeyDown);
                deckNameTextField.RegisterCallback<BlurEvent>(_ => CommitDeckNameEdit());
            }

            editDeckNameButton.clicked += ToggleDeckNameEditMode;
            RefreshDeckNameDisplay();
        }

        private void InitializeDeleteDeckModal()
        {
            if (deleteDeckButton != null)
            {
                deleteDeckButton.text = string.Empty;
                deleteDeckButton.SetEnabled(currentDeckId != Guid.Empty);
                deleteDeckButton.clicked += OpenDeleteDeckModal;
            }

            if (deleteModalHUD != null)
                deleteModalHUD.style.display = DisplayStyle.None;

            if (deleteModalConfirmButton != null)
                deleteModalConfirmButton.clicked += ConfirmDeleteDeck;

            if (deleteModalCancelButton != null)
                deleteModalCancelButton.clicked += CloseDeleteDeckModal;

            RefreshDeleteDeckButtonState();
        }

        private void OpenDeleteDeckModal()
        {
            if (currentDeckId == Guid.Empty || deleteModalHUD == null)
                return;

            if (deleteModalDeckNameLabel != null)
                deleteModalDeckNameLabel.text = string.IsNullOrWhiteSpace(currentDeckName) ? "Deck" : currentDeckName;

            deleteModalHUD.style.display = DisplayStyle.Flex;
        }

        private void CloseDeleteDeckModal()
        {
            if (deleteModalHUD != null)
                deleteModalHUD.style.display = DisplayStyle.None;
        }

        private void ConfirmDeleteDeck()
        {
            if (currentDeckId == Guid.Empty)
                return;

            CloseDeleteDeckModal();
            StartCoroutine(DeleteCurrentDeck());
        }

        private IEnumerator DeleteCurrentDeck()
        {
            if (deckService == null)
                deckService = new VortexTCG.Scripts.Features.Deck.Services.DeckService();

            Guid deckIdToDelete = currentDeckId;
            string error = null;

            yield return deckService.DeleteDeckAsync(
                deckIdToDelete,
                onSuccess: () => Debug.Log($"[DeckUI] DeleteDeck succeeded deckId={deckIdToDelete}"),
                onError: serviceError => error = serviceError
            );

            if (!string.IsNullOrWhiteSpace(error))
            {
                Debug.LogError(error);
                yield break;
            }

            ResetCurrentDeckState();
            yield return LoadUserCollectionAndDecks();
        }

        private void RefreshDeleteDeckButtonState()
        {
            if (deleteDeckButton != null)
                deleteDeckButton.SetEnabled(currentDeckId != Guid.Empty);
        }

        private void ToggleDeckNameEditMode()
        {
            if (currentDeckId == Guid.Empty || deckNameLabel == null || deckNameTextField == null)
                return;

            bool isEditing = deckNameTextField.style.display == DisplayStyle.Flex;
            if (isEditing)
            {
                CommitDeckNameEdit();
                return;
            }

            deckNameTextField.value = currentDeckName;
            SetDeckNameEditMode(true);
            deckNameTextField.schedule.Execute(() =>
            {
                deckNameTextField.Focus();
                deckNameTextField.SelectAll();
            });
        }

        private void OnDeckNameInputKeyDown(KeyDownEvent evt)
        {
            if (evt == null)
                return;

            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                evt.StopPropagation();
                CommitDeckNameEdit();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                evt.StopPropagation();
                CancelDeckNameEdit();
            }
        }

        private void CommitDeckNameEdit()
        {
            if (deckNameTextField == null)
                return;

            string newName = deckNameTextField.value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(newName))
                newName = "Deck";

            Debug.Log($"[DeckUI] CommitDeckNameEdit newName='{newName}'");

            // If the name didn't actually change, don't persist (avoids duplicate requests from UI blur/updates)
            if (string.Equals(newName, currentDeckName, StringComparison.Ordinal))
            {
                SetDeckNameEditMode(false);
                return;
            }

            SetCurrentDeckName(newName);
            SetDeckNameEditMode(false);

            if (currentDeckId != Guid.Empty)
                PersistDeckChanges();
        }

        private void CommitDeckNameEditIfNeeded()
        {
            if (deckNameTextField == null)
                return;

            if (deckNameTextField.style.display == DisplayStyle.Flex)
                CommitDeckNameEdit();
        }

        private void CancelDeckNameEdit()
        {
            if (deckNameTextField != null)
                deckNameTextField.value = currentDeckName;

            SetDeckNameEditMode(false);
        }

        private void SetCurrentDeckName(string deckName)
        {
            currentDeckName = string.IsNullOrWhiteSpace(deckName) ? "Deck" : deckName;
            RefreshDeckNameDisplay();
            RefreshDeleteDeckButtonState();

            if (deckNameContainer != null)
                deckNameContainer.style.display = DisplayStyle.Flex;

            if (selectedDeckButton != null)
                selectedDeckButton.text = currentDeckName;
        }

        private void RefreshDeckNameDisplay()
        {
            if (deckNameLabel != null)
                deckNameLabel.text = string.IsNullOrWhiteSpace(currentDeckName) ? "Deck" : currentDeckName;

            if (deckNameTextField != null)
                deckNameTextField.value = string.IsNullOrWhiteSpace(currentDeckName) ? "Deck" : currentDeckName;
        }

        private void SetDeckNameEditMode(bool isEditing)
        {
            if (deckNameLabel != null)
                deckNameLabel.style.display = isEditing ? DisplayStyle.None : DisplayStyle.Flex;

            if (deckNameTextField != null)
                deckNameTextField.style.display = isEditing ? DisplayStyle.Flex : DisplayStyle.None;

            if (editDeckNameButton != null)
                editDeckNameButton.tooltip = isEditing ? "Save deck name" : "Edit deck name";
        }

        private static CardDto CreateDeckPreviewCard(DeckCardDto card)
        {
            return new CardDto
            {
                Id = card.CardId,
                Name = card.Name,
                Price = card.Price,
                Hp = card.Hp ?? 0,
                Attack = card.Attack ?? 0,
                Cost = card.Cost,
                Description = card.Description,
                Picture = card.Picture,
                Extension = card.Extension,
                CardType = card.CardType,
                Class = card.Classes ?? new List<string>()
            };
        }

        private static List<DeckCardDto> NormalizeDeckCards(IEnumerable<DeckCardDto> cards)
        {
            if (cards == null)
                return new List<DeckCardDto>();

            return cards
                .Where(card => card != null && card.CollectionCardId != Guid.Empty)
                .GroupBy(card => card.CollectionCardId)
                .Select(group =>
                {
                    DeckCardDto first = group.First();
                    // Defensive choice: if duplicated rows are present,
                    // keep the highest declared quantity instead of summing,
                    // to avoid multiplying cards in UI/payload.
                    int totalQuantity = group.Max(item => Math.Max(item.Quantity, 0));
                    if (totalQuantity <= 0)
                        totalQuantity = 1;

                    return new DeckCardDto
                    {
                        DeckCardId = first.DeckCardId,
                        CollectionCardId = first.CollectionCardId,
                        CardId = first.CardId,
                        Name = first.Name,
                        Hp = first.Hp,
                        Attack = first.Attack,
                        Cost = first.Cost,
                        Description = first.Description,
                        Picture = first.Picture,
                        Extension = first.Extension,
                        CardType = first.CardType,
                        Price = first.Price,
                        Classes = first.Classes ?? new List<string>(),
                        Rarity = first.Rarity,
                        Quantity = totalQuantity
                    };
                })
                .ToList();
        }
    }
}
