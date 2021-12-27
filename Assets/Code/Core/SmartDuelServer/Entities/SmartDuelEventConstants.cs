namespace Code.Core.SmartDuelServer.Entities
{
    public static class SmartDuelEventConstants
    {
        //region Scopes

        public const string GlobalScope = "global";
        public const string CardScope = "card";
        public const string RoomScope = "room";

        //endregion

        //region Actions

        public const string GlobalConnectAction = "connect";
        public const string GlobalConnectErrorAction = "connect_error";
        public const string GlobalConnectTimeoutAction = "connect_timeout";
        public const string GlobalErrorAction = "error";

        public const string CardPlayAction = "play";
        public const string CardRemoveAction = "remove";
        public const string CardAttackAction = "attack";
        public const string CardDeclareAction = "declare";
        public const string CardGiveToOpponentAction = "give-to-opponent";

        public const string RoomGetDuelistsAction = "get-duelists";
        public const string RoomSpectateAction = "spectate";
        public const string RoomLeaveAction = "leave";
        public const string RoomStartAction = "start";
        public const string RoomCloseAction = "close";

        //endregion
    }
}