using System;


namespace NotifyMessageDemo
{
    public class NotifyMessage
    {
        private readonly string _skinName;
        private readonly string _headerText;
        private readonly string _bodyText;
        private readonly Action _clickAction;

        public NotifyMessage(string skinName, string headerText, string bodyText, Action clickAction)
        {
            _skinName       = skinName;
            _headerText     = headerText;
            _bodyText       = bodyText;
            _clickAction    = clickAction;
        }

        public string SkinName
        {
            get { return _skinName; }
        }
        
        public string HeaderText
        {
            get { return _headerText; }
        }

        public string BodyText
        {
            get { return _bodyText; }
        }

        public Action OnClick
        {
            get { return _clickAction; }
        }
    }
}
