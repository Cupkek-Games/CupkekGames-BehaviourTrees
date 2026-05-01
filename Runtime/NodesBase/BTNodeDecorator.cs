namespace CupkekGames.BehaviourTrees
{
    public abstract class BTNodeDecorator : BTNode
    {
        public BTNode Child;
        public override BTNode Clone()
        {
            BTNodeDecorator clone = Instantiate(this);

            if (Child != null)
            {
                clone.Child = Child.Clone();
            }

            return clone;
        }
    }
}