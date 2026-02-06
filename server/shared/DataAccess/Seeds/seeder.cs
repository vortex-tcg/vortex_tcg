using VortexTCG.DataAccess;
using VortexTCG.DataAccess.Models;

namespace VortexTCG.DataAccess.Seeds;

public static class Seeder
{
    public static void Seed(VortexDbContext db)
    {
        VortexSeeder seeder = new VortexSeeder(db);

        Guid berserkouinId = seeder.EnsureFaction(Guid.NewGuid(), "Berserkouin", "Kouin-coin", "");
        Guid cacturoquaiId = seeder.EnsureFaction(Guid.NewGuid(), "Cacturoquaï", "Eau", "");
        Guid planteId = seeder.EnsureClass(Guid.NewGuid(), "Plante");
        Guid gardienId = seeder.EnsureClass(Guid.NewGuid(), "Gardien");
        Guid guerrierId = seeder.EnsureClass(Guid.NewGuid(), "Guerrier");
        Guid nobleId = seeder.EnsureClass(Guid.NewGuid(), "Noble");
        Guid pyroId = seeder.EnsureClass(Guid.NewGuid(), "Pyro");
        Guid cryoId = seeder.EnsureClass(Guid.NewGuid(), "Cryo");
        Guid inventeurId = seeder.EnsureClass(Guid.NewGuid(), "Inventeur");
        Guid monstreId = seeder.EnsureClass(Guid.NewGuid(), "Monstre");
        Guid pirateId = seeder.EnsureClass(Guid.NewGuid(), "Pirate");
        Guid marchandId = seeder.EnsureClass(Guid.NewGuid(), "Marchand");
        Guid medecinId = seeder.EnsureClass(Guid.NewGuid(), "Médecin");
        Guid mercenaireId = seeder.EnsureClass(Guid.NewGuid(), "Mercenaire");
        Guid guerrierMonteId = seeder.EnsureClass(Guid.NewGuid(), "Guerrier monté");
        Guid pretreId = seeder.EnsureClass(Guid.NewGuid(), "Prêtre");
        Guid electriseId = seeder.EnsureClass(Guid.NewGuid(), "Electrisé");
        
        
        Guid pingrimsId = seeder.EnsureChampion(
            Guid.NewGuid(),
            "Pingrims",
            "Né dans l’ombre d’une légende, il a choisi une carrière simple: contredire son père avec une constance héroïque. Pingrims n’a pas conquis de terres, il a conquis des nerfs. À force d’enquiquiner l’Empereur, il a fini par rallier les clans rebelles, non pas par génie militaire… mais par pur talent pour rendre les gens exaspérés, solidaires, et prêts à tout “juste pour le faire suer”. Il ne marche pas sans tomber: il tombe en avant, et appelle ça une révolution.",
            30,
            "null",
            berserkouinId,
            null
        );

        Guid pingchillinId = seeder.EnsureChampion(
            Guid.NewGuid(),
            "Pingchillin Emporio",
            "Le seul manchot assez sanguinaire pour unir des clans qui se détestaient, et assez malin pour leur faire croire que c’était “le plan depuis le début”. On raconte qu’il a fait le tour complet de la banquise sans tomber une seule fois… certains jurent même qu’il n’a jamais glissé, pas même dans son propre sang. Sa couronne n’est pas en or: c’est une cicatrice. Et quand il marche, la glace se tient droite.",
            30,
            "null",
            berserkouinId,
            null
        );

        Guid potPourrisId = seeder.EnsureChampion(
            Guid.NewGuid(),
            "Pot-Pourris",
            "Un croisement entre toutes les boissons",
            30,
            "null",
            cacturoquaiId,
            null
        );

        Guid hydraSigmaId = seeder.EnsureChampion(
            Guid.NewGuid(),
            "Hydra Sigma",
            "La reine des cactus et aussi la créature la plus pure du monde composé à 100,100% d'eau",
            30,
            "null",
            cacturoquaiId,
            null
        );

        Guid hacheurBanquiseId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Hacheur de Banquise",
            10, 1, 2, 1,
            "Frappe fort, vise mal, découpe quand même… souvent le décor.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), hacheurBanquiseId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), hacheurBanquiseId, guerrierId);

        Guid fracasseurGivreId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Fracasseur de Givre",
            30, 3, 4, 3,
            "Brise tout ce qui bouge, et ce qui ne bouge pas. Surtout ses alliés.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), fracasseurGivreId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), fracasseurGivreId, guerrierId);

        Guid fendeurPolaireId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Fendeur polaire",
            20, 2, 2, 2,
            "Se jette sur la cible avec panache, glisse avant l’impact, et recommence.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), fendeurPolaireId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), fendeurPolaireId, guerrierId);

        Guid executeurGelId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Executeur du gel",
            50, 4, 6, 5,
            "Prononce la sentence d’une voix grave… puis oublie lequel il devait frapper.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), executeurGelId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), executeurGelId, guerrierId);

        Guid berserkouinGlacesId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Berserkouin des Glaces",
            80, 6, 8, 8,
            "Rage pure sous la neige: quand il s’énerve, même sa banquise demande pardon.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), berserkouinGlacesId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), berserkouinGlacesId, guerrierId);
        Guid mordeurBraiseId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Mordeur de Braise",
            10, 2, 1, 1,
            "Mors à pleines dents, réalise trop tard que “c’était chaud” n’est pas une métaphore.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), mordeurBraiseId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), mordeurBraiseId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), mordeurBraiseId, pyroId);

        Guid ravageurIncandescentId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Ravageur incandescent",
            30, 1, 5, 3,
            "Un incendie sur pattes: il brûle l’ennemi, et parfois le plan de bataille.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), ravageurIncandescentId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), ravageurIncandescentId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), ravageurIncandescentId, pyroId);

        Guid chasseurFlammeId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Chasseur de flamme",
            20, 3, 2, 2,
            "Traque la chaleur avec obsession… quitte à courir après une torche.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), chasseurFlammeId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), chasseurFlammeId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), chasseurFlammeId, pyroId);

        Guid ecorcheurFoyerId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Ecorcheur du foyer",
            50, 6, 4, 5,
            "Aime les combats “au corps à corps”. Le foyer, c’est juste sa zone de confort.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), ecorcheurFoyerId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), ecorcheurFoyerId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), ecorcheurFoyerId, pyroId);

        Guid berserkouinCrocId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Berserkouin du Croc",
            70, 8, 6, 7,
            "Charge en hurlant, mord ce qu’il trouve, puis s’étonne du goût.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), berserkouinCrocId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), berserkouinCrocId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), berserkouinCrocId, pyroId);
        Guid marteleurNordId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Marteleur du nord",
            40, 5, 4, 4,
            "Frappe comme un tonnerre… et pense que la finesse est une maladie.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), marteleurNordId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), marteleurNordId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), marteleurNordId, electriseId);

        Guid briseurCieuxId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Briseur de cieux",
            20, 4, 1, 2,
            "Promet de fendre les nuages. Se contente souvent de fendre les casques.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), briseurCieuxId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), briseurCieuxId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), briseurCieuxId, electriseId);

        Guid enclumeurSacreId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Enclumeur sacré",
            90, 9, 7, 9,
            "Chaque coup est une prière. Une prière très bruyante et très violente.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), enclumeurSacreId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), enclumeurSacreId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), enclumeurSacreId, electriseId);

        Guid foudroyeurMarteauId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Foudroyeur du marteau",
            40, 3, 5, 4,
            "Électrise le champ de bataille. Y compris lui-même, par inadvertance.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), foudroyeurMarteauId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), foudroyeurMarteauId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), foudroyeurMarteauId, electriseId);

        Guid berserkouinRuniqueId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Berserkouin runique",
            100, 8, 10, 10,
            "Grave des runes sur son marteau: personne ne sait ce que ça veut dire, lui non plus.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), berserkouinRuniqueId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), berserkouinRuniqueId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), berserkouinRuniqueId, electriseId);
        Guid chevaucheurNeigesId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Chevaucheur des neiges",
            20, 3, 1, 2,
            "Monte sa bête avec fierté… dans le mauvais sens, mais avec conviction.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), chevaucheurNeigesId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), chevaucheurNeigesId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), chevaucheurNeigesId, guerrierMonteId);
        Guid lancierTigresId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Lancier des tigres",
            40, 3, 2, 4,
            "Lance sa lance avant d’être à portée. La surprise fait partie de la stratégie.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), lancierTigresId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), lancierTigresId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), lancierTigresId, guerrierMonteId);

        Guid pilleurDosTigreId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Pilleur à dos de tigre",
            20, 2, 4, 2,
            "Pille vite, fuit vite, tombe souvent. Le tigre, lui, est habitué.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), pilleurDosTigreId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), pilleurDosTigreId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), pilleurDosTigreId, guerrierMonteId);

        Guid traqueurCretesId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Traqueur des crêtes",
            60, 10, 3, 6,
            "Se croit discret. Ses pas font le bruit d’une avalanche enthousiaste.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), traqueurCretesId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), traqueurCretesId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), traqueurCretesId, guerrierMonteId);

        Guid berserkerCavalierId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Berserker Cavalier",
            90, 6, 10, 9,
            "Charge au galop, hurle à la lune, oublie de tenir la selle… et frappe quand même.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), berserkerCavalierId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), berserkerCavalierId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), berserkerCavalierId, guerrierMonteId);
        Guid sentinelleGlacesId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Sentinelle des glaces",
            10, 1, 1, 1,
            "Garde la ligne avec sérieux, jusqu’à ce qu’un flocon le distrait.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), sentinelleGlacesId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), sentinelleGlacesId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), sentinelleGlacesId, gardienId);

        Guid gardienBanquiseId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Gardien de banquise",
            30, 4, 1, 3,
            "Protège les siens coûte que coûte… surtout en servant de bouclier involontaire.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), gardienBanquiseId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), gardienBanquiseId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), gardienBanquiseId, gardienId);

        Guid rempartPolaireId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Rempart polaire",
            60, 8, 2, 6,
            "Solide comme un mur. Un mur qui avance en grognant et en bavant.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), rempartPolaireId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), rempartPolaireId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), rempartPolaireId, gardienId);

        Guid defenseurFjordId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Défenseur du Fjord",
            70, 6, 8, 7,
            "Défend le passage avec honneur. Et avec des coups de tête très personnels.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), defenseurFjordId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), defenseurFjordId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), defenseurFjordId, gardienId);

        Guid berserkouinBastionId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Berserkouin du Bastion",
            100, 10, 6, 10,
            "Le dernier à tomber, le premier à charger. Logique douteuse, efficacité réelle.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), berserkouinBastionId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), berserkouinBastionId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), berserkouinBastionId, gardienId);
        Guid fanatiqueOrquesId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Fanatique Orques",
            30, 1, 6, 3,
            "Hurle des prières incompréhensibles et tape très fort: la foi fait le reste.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), fanatiqueOrquesId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), fanatiqueOrquesId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), fanatiqueOrquesId, pretreId);

        Guid massacreurTotemiqueId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Massacreur Totémique",
            10, 1, 2, 1,
            "Porte un totem sacré. S’en sert surtout comme massue.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), massacreurTotemiqueId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), massacreurTotemiqueId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), massacreurTotemiqueId, pretreId);

        Guid ravageurDefensesId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Ravageur des défenses",
            40, 3, 4, 4,
            "Ne comprend pas le mot “défense”. Il détruit donc tout, par précaution.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), ravageurDefensesId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), ravageurDefensesId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), ravageurDefensesId, pretreId);

        Guid pretreGuerrierOrquesId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Prêtre-guerrier orques",
            80, 4, 7, 8,
            "Bénit les armes, maudit les ennemis, confond parfois les deux.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), pretreGuerrierOrquesId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), pretreGuerrierOrquesId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), pretreGuerrierOrquesId, pretreId);

        Guid berserkouinDieuOrquesId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Berserkouin du dieu orques",
            100, 10, 10, 10,
            "Se bat pour la gloire divine… et parce que ça l’amuse beaucoup trop.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), berserkouinDieuOrquesId, berserkouinId);
        seeder.EnsureClassCard(Guid.NewGuid(), berserkouinDieuOrquesId, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), berserkouinDieuOrquesId, pretreId);
        Guid chateauBarbarieId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Château Barbarie",
            50, 9, 1, 5,
            "Ancien gardien de réservoir, transformé en homme-cactus “stockage optimisé”. Il a tellement gonflé qu’on l’appelle Château, et il exige qu’on toque avant d’arroser “ses étages”.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), chateauBarbarieId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), chateauBarbarieId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), chateauBarbarieId, gardienId);

        Guid elTequiloId = seeder.EnsureCard(
            Guid.NewGuid(),
            "El Téquilo",
            20, 3, 2, 2,
            "Ex-barman reconverti en cobaye, “désinfecté” un peu trop fort dans la cuve. Depuis il marche au soleil comme un héros et boit l’eau en shot, avec regard de duel.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), elTequiloId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), elTequiloId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), elTequiloId, guerrierId);

        Guid sirDanielsId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Sir Daniel’s",
            80, 5, 7, 8,
            "Un comptable qui rêvait d’élégance, fusionné avec cactus et copeaux de fût. Il parle comme un noble, juge ta gourde, et dit “s’il vous plaît” avec menace.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), sirDanielsId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), sirDanielsId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), sirDanielsId, nobleId);

        Guid iceCactCubeId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Ice Cact Cube",
            10, 2, 1, 1,
            "Ancien frigoriste, devenu homme-cactus cryo après un test “eau fraîche permanente”. Il transpire du froid, met l’ambiance “frissons”, et refuse de fondre même socialement.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), iceCactCubeId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), iceCactCubeId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), iceCactCubeId, cryoId);

        Guid blackThornId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Black Thorn",
            40, 6, 2, 4,
            "Ex-vigile de boîte, croisé avec cactus et “concentré de nuit”. Il absorbe la lumière, apparaît sans prévenir, et ne sourit que quand quelqu’un renverse de l’eau (donc jamais).",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), blackThornId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), blackThornId, planteId);

        Guid domPiquignonId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Dom Piquignon",
            40, 9, 1, 4,
            "Un cactus très peu éduqué à été croisé avec un vin rouge classé, comment s’est arrivé ? les chercheurs se sont trompés de bouteilles, il est pas très intelligent mais qu’est ce qu’il est côté..",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), domPiquignonId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), domPiquignonId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), domPiquignonId, nobleId);

        Guid vinGazEpineId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Vin Gaz’Epine",
            30, 5, 2, 3,
            "Ancien mécano, victime d’un protocole “secouer pour mélanger”. Il est maintenant gazeux par nature: il siffle, gonfle, et ses épines sautent comme des bouchons.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), vinGazEpineId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), vinGazEpineId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), vinGazEpineId, inventeurId);

        Guid limonadeId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Limonade",
            10, 3, 1, 1,
            "Ex-mascotte publicitaire devenue homme-cactus au sirop sucré. Il est adorable, hyperactif, et capable de raconter une blague pendant qu’on te vole ta gourde.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), limonadeId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), limonadeId, planteId);

        Guid experience627Id = seeder.EnsureCard(
            Guid.NewGuid(),
            "Expérience 627",
            100, 10, 7, 10,
            "Pourquoi 627 ? bah parcequ’il y en a eu 626 avant à ton avis.\nIl s’agit d’un croisement entre une plante carnivore, un cactus et beaucoup de boissons énergisante !",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), experience627Id, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), experience627Id, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), experience627Id, guerrierId);
        seeder.EnsureClassCard(Guid.NewGuid(), experience627Id, monstreId);

        Guid antolytesId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Antolytes",
            60, 8, 4, 6,
            "Qu'est ce qu'il se passe quand on gave un cactus d'électrolytes ? Il devient un véritable générateur capable de courir un marathon sans même transpirer une goutte !",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), antolytesId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), antolytesId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), antolytesId, guerrierId);

        Guid captainHoublonId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Captain Houblon",
            100, 7, 10, 10,
            "Le plus grand guitariste de punk à été sauvé en étant croisé avec un cactus, le problème ? Les scientifique ont fait tombé deux pintes dans la cuve au moment du croisement, il est plus vraiment clair depuis ce jour la..",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), captainHoublonId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), captainHoublonId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), captainHoublonId, guerrierId);

        Guid poilDeCarotteId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Poil de Carotte",
            50, 1, 1, 5,
            "C’est le cactus le plus énervant de l’ouest !\nIl a été croisé avec un jus de carotte fort en vitamine C et fait chier tout le monde avec ses discussion sans aucun sens.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), poilDeCarotteId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), poilDeCarotteId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), poilDeCarotteId, guerrierId);

        Guid rubyRoubaixId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Ruby Roubaix",
            30, 5, 2, 3,
            "Ruby était la Cactus la plus jolie de tout les Cacturoquai, mais ça s’était avant de s’enfiler des litres de Gin, aujourd’hui c’est à peine si elle arrivent à entrer dans son pot..",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), rubyRoubaixId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), rubyRoubaixId, planteId);

        Guid champiPunchId = seeder.EnsureCard(
            Guid.NewGuid(),
            "ChampiPunch",
            10, 10, 2, 1,
            "Son père était une Morille, sa mère un ananas, et lui a été mixé dans une grande quantité de Rhum, il est imbuvable mais vous passerez de chouette soirée au moins !",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), champiPunchId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), champiPunchId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), champiPunchId, guerrierId);

        Guid colaTomId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Cola-Tom",
            50, 6, 4, 5,
            "Un chercheur à oublié un pack de six cola dans son réacteur nucléaire portatif, depuis ça donnée une boisson délicieusement hydratante pour les cactus, dommage que ça fasse tomber les épines…",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), colaTomId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), colaTomId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), colaTomId, guerrierId);

        Guid captainMortGagneId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Cap’tain Mort Gagne",
            30, 4, 3, 3,
            "Frère de Captain Houblon, ex-pillard de caravanes recyclé en homme-cactus. Il vole les citernes “par tradition familiale”, puis oublie où il les a planquées, donc il revol.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), captainMortGagneId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), captainMortGagneId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), captainMortGagneId, pirateId);

        Guid senorCuervoId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Señor Cuervo",
            40, 4, 3, 4,
            "Ex-coursier devenu homme-cactus après qu’un corbeau ait “participé” au protocole. Depuis, il apporte des nouvelles, des ennuis, et parfois les deux dans la même phrase.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), senorCuervoId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), senorCuervoId, planteId);

        Guid jeanMarcheurId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Jean Marcheur",
            10, 3, 2, 1,
            "Ex-randonneur transformé avec un mélange “endurance maximale”. Il marche tout le temps, même quand il dort, et quand on lui demande pourquoi il répond: “Parce que.”",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), jeanMarcheurId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), jeanMarcheurId, planteId);

        Guid chivasRegaleId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Chiv’as Régale",
            80, 8, 3, 8,
            "Ex-snob du vieux monde, devenu homme-cactus premium en cave climatisée. Il t’offre une gorgée incroyable, puis te fait la morale sur ta façon de tenir ta gourde.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), chivasRegaleId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), chivasRegaleId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), chivasRegaleId, nobleId);

        Guid maitreJagerId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Maitre-Jäger",
            40, 5, 5, 4,
            "Ex-chasseur de primes injecté d’herbes et d’épices “pour l’instinct”. Il traque les voleurs d’eau, renifle les mensonges, et te facture du regard.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), maitreJagerId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), maitreJagerId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), maitreJagerId, mercenaireId);

        Guid bacarDuneId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Bacar’Dune",
            30, 4, 2, 3,
            "Ex-contrebandeur transformé en cactus furtif après trop de vie dans des caisses. Il vend des cartes vers des oasis… qui deviennent mystérieusement “plus loin” après paiement.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), bacarDuneId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), bacarDuneId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), bacarDuneId, pirateId);

        Guid smirnIcePiqueeId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Smirn’Ice Piquée",
            30, 5, 2, 3,
            "Ex-infirmière de terrain devenue homme-cactus cryo avec une dose “ça pique mais c’est normal”. Ses épines anesthésient, sa sève rafraîchit, et ta mémoire signe une pause.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), smirnIcePiqueeId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), smirnIcePiqueeId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), smirnIcePiqueeId, medecinId);
        seeder.EnsureClassCard(Guid.NewGuid(), smirnIcePiqueeId, cryoId);

        Guid heineEpineId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Heine-Epine",
            60, 7, 4, 6,
            "Ex-agent de sécurité transformé en cactus “ordre et discipline”. Il tamponne les gourdes, fait des rondes, et rêve d’un monde où l’eau est en formulaire PDF.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), heineEpineId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), heineEpineId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), heineEpineId, guerrierId);

        Guid guinnEssaimId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Guinn’Essaim",
            100, 10, 8, 10,
            "Ex-démolisseur nourri à un concentré sombre “pour la masse”. Il est devenu un tank humain-cactus, suivi d’un essaim attiré par sa sève épaisse. Il n’avance pas: il débarque.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), guinnEssaimId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), guinnEssaimId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), guinnEssaimId, gardienId);
        seeder.EnsureClassCard(Guid.NewGuid(), guinnEssaimId, guerrierId);

        Guid grisGousseId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Gris Gousse",
            10, 1, 2, 1,
            "Ex-cuisinier chimiste qui a confondu le labo et la cuisine pendant la transformation. Maintenant homme-cactus alchimiste: il soigne parfois, il hallucine souvent, mais il assaisonne toujours.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), grisGousseId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), grisGousseId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), grisGousseId, inventeurId);

        Guid belVendeurId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Bel Vendeur",
            30, 6, 1, 3,
            "Ex-commercial devenu homme-cactus “diplomatie”. Il te charme, tu lui donnes ton eau, et tu te remercies toi-même après, ce qui est médicalement préoccupant.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), belVendeurId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), belVendeurId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), belVendeurId, marchandId);

        Guid havaneCloubeId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Havane Cloube",
            70, 8, 4, 7,
            "Ex-DJ de bunker transformé en cactus après une fuite parfumée dans la cuve. Il a ouvert un club autour de lui: on danse, on complote, et l’eau coûte un secret plus un supplément ambiance.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), havaneCloubeId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), havaneCloubeId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), havaneCloubeId, marchandId);

        Guid bailisseCremeuseId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Baïlisse Crémeuse",
            50, 4, 3, 5,
            "Ex-médecin devenue homme-cactus à la crème expérimentale “désinfectante”. Elle te soigne nickel, mais te juge pendant tout le process, comme si ton sang lui faisait perdre son temps.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), bailisseCremeuseId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), bailisseCremeuseId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), bailisseCremeuseId, medecinId);

        Guid cahlouaiId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Cahlouaï",
            20, 4, 1, 2,
            "Ex-veilleur de nuit fusionné à cactus et mélange noir interdit. Il ne dort plus jamais, invente des plans à 3h du matin, puis oublie le plan à 3h01 mais garde la confiance.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), cahlouaiId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), cahlouaiId, planteId);

        Guid krakeneSablesId = seeder.EnsureCard(
            Guid.NewGuid(),
            "Krakène des Sables",
            100, 10, 10, 10,
            "Ex-plombier tombé dans une canalisation cassée le jour du grand test. Il est devenu un homme-cactus tentaculaire: il sent l’eau à distance et surgit quand quelqu’un prononce “hydratation”.",
            "null",
            Extension.BASIC,
            CardType.GUARD
        );
        seeder.EnsureFactionCard(Guid.NewGuid(), krakeneSablesId, cacturoquaiId);
        seeder.EnsureClassCard(Guid.NewGuid(), krakeneSablesId, planteId);
        seeder.EnsureClassCard(Guid.NewGuid(), krakeneSablesId, monstreId);
        
        seeder.Save();
        
        DateTime utcDate = DateTime.UtcNow;
        string actor = "Seeder";

        Guid[] starterChampionIds = new Guid[] { potPourrisId, hydraSigmaId, pingchillinId, pingrimsId };

        Guid[] starterCardIds = new Guid[]
        {
            hacheurBanquiseId, fracasseurGivreId, fendeurPolaireId, executeurGelId, berserkouinGlacesId,
            mordeurBraiseId, ravageurIncandescentId, chasseurFlammeId, ecorcheurFoyerId, berserkouinCrocId,
            marteleurNordId, briseurCieuxId, enclumeurSacreId, foudroyeurMarteauId, berserkouinRuniqueId,
            chevaucheurNeigesId, lancierTigresId, pilleurDosTigreId, traqueurCretesId, berserkerCavalierId,
            sentinelleGlacesId, gardienBanquiseId, rempartPolaireId, defenseurFjordId, berserkouinBastionId,
            fanatiqueOrquesId, massacreurTotemiqueId, ravageurDefensesId, pretreGuerrierOrquesId, berserkouinDieuOrquesId,
            chateauBarbarieId, elTequiloId, sirDanielsId, iceCactCubeId, blackThornId, domPiquignonId, vinGazEpineId, limonadeId,
            experience627Id, antolytesId, captainHoublonId, poilDeCarotteId, rubyRoubaixId, champiPunchId, colaTomId,
            captainMortGagneId, senorCuervoId, jeanMarcheurId, chivasRegaleId, maitreJagerId, bacarDuneId,
            smirnIcePiqueeId, heineEpineId, guinnEssaimId, grisGousseId, belVendeurId, havaneCloubeId,
            bailisseCremeuseId, cahlouaiId, krakeneSablesId
        };

        seeder.SeedStarterCollectionForUser(db, "john.doe@email.com", starterChampionIds, starterCardIds, utcDate, actor);
        seeder.SeedStarterCollectionForUser(db, "jane.doe@email.com", starterChampionIds, starterCardIds, utcDate, actor);
        Guid[] deckBerserkouinCardIds = new[]
        {
            hacheurBanquiseId, fendeurPolaireId, fracasseurGivreId, executeurGelId, berserkouinGlacesId,
            mordeurBraiseId, ravageurIncandescentId, chasseurFlammeId, ecorcheurFoyerId, berserkouinCrocId,
            marteleurNordId, briseurCieuxId, enclumeurSacreId, foudroyeurMarteauId, berserkouinRuniqueId,
            chevaucheurNeigesId, lancierTigresId, pilleurDosTigreId, traqueurCretesId, berserkerCavalierId,
            sentinelleGlacesId, gardienBanquiseId, rempartPolaireId, defenseurFjordId, berserkouinBastionId,
            fanatiqueOrquesId, massacreurTotemiqueId, ravageurDefensesId, pretreGuerrierOrquesId, berserkouinDieuOrquesId
        };

        Guid[] deckCacturoquaiCardIds = new[]
        {
            chateauBarbarieId, elTequiloId, sirDanielsId, iceCactCubeId, blackThornId, domPiquignonId, vinGazEpineId, limonadeId,
            experience627Id, antolytesId, captainHoublonId, poilDeCarotteId, rubyRoubaixId, champiPunchId, colaTomId,
            captainMortGagneId, senorCuervoId, jeanMarcheurId, chivasRegaleId, maitreJagerId, bacarDuneId,
            smirnIcePiqueeId, heineEpineId, guinnEssaimId, grisGousseId, belVendeurId, havaneCloubeId,
            bailisseCremeuseId, cahlouaiId, krakeneSablesId
        };
        seeder.SeedDeckForUser(
            db,
            email: "john.doe@email.com",
            deckName: "Deck Berserkouin",
            factionId: berserkouinId,
            championId: pingchillinId,
            cardIds: deckBerserkouinCardIds,
            utcDate: utcDate,
            actor: actor
        );

        seeder.SeedDeckForUser(
            db,
            email: "john.doe@email.com",
            deckName: "Deck Cacturoquaï",
            factionId: cacturoquaiId,
            championId: potPourrisId,
            cardIds: deckCacturoquaiCardIds,
            utcDate: utcDate,
            actor: actor
        );
        seeder.SeedDeckForUser(
            db,
            email: "jane.doe@email.com",
            deckName: "Road Rage",
            factionId: berserkouinId,
            championId: pingrimsId,
            cardIds: deckBerserkouinCardIds,
            utcDate: utcDate,
            actor: actor
        );

        seeder.SeedDeckForUser(
            db,
            email: "jane.doe@email.com",
            deckName: "Virtual insanity",
            factionId: cacturoquaiId,
            championId: hydraSigmaId,
            cardIds: deckCacturoquaiCardIds,
            utcDate: utcDate,
            actor: actor
        );

        seeder.Save();
    }
    
}