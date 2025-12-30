using VortexTCG.Game.DTO;

namespace VortexTCG.Game.Interface {    

    public interface IRoomActionEventListener {
        public void sendDrawCardsData(DrawCardsResultDTO data);
        public void sendBattleResolveData(BattlesDataDto data, Guid attackerId, Guid defenderId);

    }

}