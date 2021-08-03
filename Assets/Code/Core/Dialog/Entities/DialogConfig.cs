using System;

namespace Code.Core.Dialog.Entities
{
    public class DialogConfig
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PositiveText { get; set; }
        public Action PositiveAction { get; set; }
    }
}