namespace Lpubsppop01.ReplaceCode.Actors
{
    class LoadBalancerActor : Actor
    {
        Actor[] actors;
        int iNextActor;

        public LoadBalancerActor(params Actor[] actors)
        {
            this.actors = actors;
        }

        protected override void OnMessage(object message)
        {
            actors[iNextActor].MessageQueue.Enqueue(message);
            iNextActor = (iNextActor < actors.Length - 1) ? iNextActor + 1 : 0;
        }
    }
}
