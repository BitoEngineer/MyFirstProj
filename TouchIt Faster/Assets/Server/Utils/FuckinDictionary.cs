using System;
using System.Collections.Generic;

namespace Assets.Server.Utils
{
    public class FuckinDictionary<X, Y>
    {
        private object _locker = new object();
        private Dictionary<X, Y> _dic = new Dictionary<X, Y>();

        public bool TryRemove(X x, out Y y)
        {
            y = default(Y);

            lock (_locker)
            {
                if (_dic.ContainsKey(x))
                {
                    y = _dic[x];
                    _dic.Remove(x);
                    return true;
                }
            }

            return false;
        }

        public bool TryGetValue(X x, out Y y)
        {
            y = default(Y);

            lock (_locker)
            {
                if (_dic.ContainsKey(x))
                {
                    y = _dic[x];
                    return true;
                }
            }

            return false;
        }

        public void AddOrUpdate(X x, Y y, Func<X, Y, Y> func)
        {
            lock (_locker)
            {
                if (_dic.ContainsKey(x))
                {
                    _dic[x] = func(x, _dic[x]);
                }
                else
                {
                    _dic.Add(x, y);
                }
            }
        }

        internal bool ContainsKey(int packetID)
        {
            throw new NotImplementedException();
        }
    }

}
