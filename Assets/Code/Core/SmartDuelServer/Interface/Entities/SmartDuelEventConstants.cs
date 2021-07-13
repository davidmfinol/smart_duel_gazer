namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities
{
    public class SmartDuelEventConstants
    {
        //region Scopes

        public const string globalScope = "global";
        public const string cardScope = "card";
        public const string roomScope = "room";

        //endregion

        //region Actions

        public const string globalConnectAction = "connect";
        public const string globalConnectErrorAction = "connect_error";
        public const string globalConnectTimeoutAction = "connect_timeout";
        public const string globalConnectingAction = "connecting";
        public const string globalDisconnectAction = "disconnect";
        public const string globalErrorAction = "error";
        public const string globalReconnectAction = "reconnect";

        public const string cardPlayAction = "play";
        public const string cardRemoveAction = "remove";

        public const string roomCreateAction = "create";
        public const string roomCloseAction = "close";
        public const string roomJoinAction = "join";
        public const string roomStartAction = "start";
        public const string roomSurrenderAction = "surrender";

        //endregion
    }
}