using System.Collections.Generic;

namespace Suburb.ExpressRouter
{
    public class Selector
    {
        // inverse isNothingIncludeConcrete
        private bool isAllExcludeConcrete;
        
        private readonly HashSet<string> concrete = new();
        
        public const string NOTHING = ".null.";
        public const string ALL = "*";
        
        public Selector Include(string id)
        {
            id = TransformId(id);
            
            if (id == ALL)
            {
                concrete.Clear();
                isAllExcludeConcrete = true;
                return this;
            }

            switch (isAllExcludeConcrete)
            {
                case true when concrete.Contains(id):
                    concrete.Remove(id);
                    return this;
                case false:
                    concrete.Add(id);
                    return this;
                default:
                    return this;
            }
        }

        public Selector Exclude(string id)
        {
            id = TransformId(id);
            
            if (id == ALL)
            {
                concrete.Clear();
                isAllExcludeConcrete = false;
                return this;
            }
            
            switch (isAllExcludeConcrete)
            {
                case false when concrete.Contains(id):
                    concrete.Remove(id);
                    return this;
                case true:
                    concrete.Add(id);
                    return this;
                default:
                    return this;
            }
        }

        internal bool Match(string id)
        {
            id = TransformId(id);
            
            if (id == ALL)
                return isAllExcludeConcrete && concrete.Count == 0;
            
            if (isAllExcludeConcrete)
                return !concrete.Contains(id);
            return concrete.Contains(id);    
        }

        internal static string TransformId(string id)
        {
            return string.IsNullOrEmpty(id) ? NOTHING : id;
        }
        
        public static Selector All()
        {
            return new Selector().Include(ALL);
        }

        public static Selector One(string id)
        {
            return new Selector().Include(id);
        }
    }
}