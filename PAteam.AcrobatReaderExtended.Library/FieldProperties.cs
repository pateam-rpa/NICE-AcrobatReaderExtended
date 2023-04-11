using Direct.Shared;


namespace Direct.PDFExtended.Library
{
    #region Supporting Classes

    [DirectDom("PDF Field Properties", "General")]
    public class FieldProperties : DirectComponentBase
    {
        protected PropertyHolder<bool> _IsMultiline = new PropertyHolder<bool>("Is Multiline");
        protected PropertyHolder<bool> _IsRequired = new PropertyHolder<bool>("Is Required");
        protected PropertyHolder<bool> _IsReadonly = new PropertyHolder<bool>("Is Read Only");
        protected PropertyHolder<string> _TextAlignment = new PropertyHolder<string>("Text Alignment");
        protected PropertyHolder<string> _CustomFont = new PropertyHolder<string>("Custom Font Path");
        protected PropertyHolder<bool> _ShouldScroll = new PropertyHolder<bool>("Should Scroll Long Text");
        protected PropertyHolder<int> _FontSize = new PropertyHolder<int>("Font Size");


        [DirectDom("Is Multiline")]
        [DesignTimeInfo("Is Multiline")]
        public bool IsMultiline
        {
            get { return _IsMultiline.TypedValue; }
            set { _IsMultiline.TypedValue = value; }
        }

        [DirectDom("Is Required")]
        [DesignTimeInfo("Is Required")]
        public bool IsRequired
        {
            get { return _IsRequired.TypedValue; }
            set { _IsRequired.TypedValue = value; }
        }

        [DirectDom("Is Read Only")]
        [DesignTimeInfo("Is Read Only")]
        public bool IsReadOnly
        {
            get { return _IsReadonly.TypedValue; }
            set { _IsReadonly.TypedValue = value; }
        }

        [DirectDom("Text Alignment")]
        [DesignTimeInfo("Text Alignment")]
        public string TextAlignment
        {
            get { return _TextAlignment.TypedValue; }
            set { _TextAlignment.TypedValue = value; }
        }

        [DirectDom("Custom Font Path")]
        [DesignTimeInfo("Custom Font Path")]
        public string CustomFont
        {
            get { return _CustomFont.TypedValue; }
            set { _CustomFont.TypedValue = value; }
        }

        [DirectDom("Should Scroll Long Text")]
        [DesignTimeInfo("Should Scroll Long Text")]
        public bool ShouldScroll
        {
            get { return _ShouldScroll.TypedValue; }
            set { _ShouldScroll.TypedValue = value; }
        }

        [DirectDom("Font Size")]
        [DesignTimeInfo("Font Size")]
        public int FontSize
        {
            get { return _FontSize.TypedValue; }
            set { _FontSize.TypedValue = value; }
        }


        public FieldProperties()
        {

        }

        public FieldProperties(IProject project) : base(project)
        {
            ShouldScroll = true;
            FontSize = 10;
            TextAlignment = "left";
        }

    }

    #endregion
}