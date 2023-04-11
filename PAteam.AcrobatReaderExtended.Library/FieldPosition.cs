using Direct.Shared;


namespace Direct.PDFExtended.Library
{
    #region Supporting Classes

    [DirectDom("PDF Field Position", "General")]
    public class FieldPosition : DirectComponentBase
    {
        protected PropertyHolder<double> _X = new PropertyHolder<double>("X");
        protected PropertyHolder<double> _Y = new PropertyHolder<double>("Y");


        [DirectDom("X")]
        [DesignTimeInfo("X")]
        public double X
        {
            get { return _X.TypedValue; }
            set { _X.TypedValue = value; }
        }

        [DirectDom("Y")]
        [DesignTimeInfo("Y")]
        public double Y
        {
            get { return _Y.TypedValue; }
            set { _Y.TypedValue = value; }
        }

        public FieldPosition()
        {

        }

        public FieldPosition(IProject project) : base(project)
        {

        }

    }

    #endregion
}