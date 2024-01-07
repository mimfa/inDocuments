using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiMFa.UIL.Searcher.Model
{
    public enum SourceState
    {
        Local = 0,
        Net = 1,
        Web = 2
    }
    public delegate void ForEachHandler(string items);
    public delegate void LogHandler(int type, string message);
    public delegate string MatchHandler(string addr);
    public delegate string MatchHandlerByPath(string addr, out string path);
    public class Sources : List<string>
    {
        public SourceState State = SourceState.Local;
        public List<IEnumerable<string>> EnumerableItems = new List<IEnumerable<string>>();
        public int Maximum => base.Count + EnumerableItems.Count;

        public Sources(SourceState state) { State = state; }


        public int Value { get; private set; } = 0;
        public IEnumerable<string> GetEnumerable()
        {
            foreach (var item in this)
            {
                Value++;
                yield return item;
            }
            foreach (var item in EnumerableItems)
            {
                foreach (var enumr in item)
                    yield return enumr;
                Value++;
            }
        }
    }
}
