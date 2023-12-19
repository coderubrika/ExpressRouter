namespace Suburb.ExpressRouter
{
    public class Rule
    {
        private readonly Selector from;
        private readonly Selector to;
        
        public Rule(Selector from, Selector to)
        {
            this.from = from;
            this.to = to;
        }
        
        internal bool Match(string fromId, string toId)
        {
            return from.Match(fromId) && to.Match(toId);
        }

        public static Rule AllToAll()
        {
            return new Rule(Selector.All(), Selector.All());
        }

        public static Rule AllToThis(string id)
        {
            return new Rule(Selector.All(), new Selector().Include(id));
        }
        
        public static Rule ThisToAll(string id)
        {
            return new Rule(new Selector().Include(id), Selector.All());
        }
        
        public static Rule AToB(string aId, string bId)
        {
            return new Rule(new Selector().Include(aId), new Selector().Include(bId));
        }
    }
}