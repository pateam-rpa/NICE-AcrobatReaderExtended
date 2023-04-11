using Direct.Shared;


namespace Direct.PDFExtended.Library
{
    #region Supporting Classes


    [DirectDom("PDF Field Size", "General")]
    public class FieldSize : DirectComponentBase
    {
        protected PropertyHolder<double> _Width = new PropertyHolder<double>("Width");
        protected PropertyHolder<double> _Height = new PropertyHolder<double>("Height");


        [DirectDom("Width")]
        [DesignTimeInfo("Width")]
        public double Width
        {
            get { return _Width.TypedValue; }
            set { _Width.TypedValue = value; }
        }

        [DirectDom("Height")]
        [DesignTimeInfo("Height")]
        public double Height
        {
            get { return _Height.TypedValue; }
            set { _Height.TypedValue = value; }
        }

        public FieldSize()
        {

        }

        public FieldSize(IProject project) : base(project)
        {

        }

    }

    #endregion
}