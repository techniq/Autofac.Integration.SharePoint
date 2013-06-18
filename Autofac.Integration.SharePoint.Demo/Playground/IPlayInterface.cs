using System;

namespace Autofac.Integration.SharePoint.Demo.Playground
{
    public interface IPlayInterface
    {
        string Uid { get; }
    }

    public class PlayInterface : IPlayInterface
    {
        private readonly string _uid;

        public PlayInterface() : this(Guid.NewGuid().ToString("N"))
        {
        }

        public PlayInterface(string uid)
        {
            _uid = uid;
        }

        #region IPlayInterface Members

        public string Uid
        {
            get { return _uid; }
        }

        #endregion
    }
}