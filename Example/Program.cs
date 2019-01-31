using System;
using System.Linq;
using Mechanix; //Core of Mechanix
using Mechanix.Laws; //Support for force evaluation laws

namespace Example
{
    class Program
    {
        enum PendulumEntities { Axis, Mass };

        const double dt = 0.0001;
        const double freeFallAcceleration = 9.8;

        static void Main(string[] args)
        {
            //PhysicalContext is the core class represents set of entities and
            //set of force evaluation laws binded to it.
            //Type parameter represents the type of keys used to adding and retrieving entities.
            var context = new PhysicalContext<string>
            (
                timePerTick: dt, //The time, that is considered to be as small, as force values are uniform.
                //The smaller timePerTick is, the better precision we get.
                //Be aware, time for evaluating state of entities after certain period of time
                //is proportional to (timePerTick)^-1.
                capacity: 1 //Number of entities required to be added to context.
            );

            //Adding entity
            var freeFallEntity = new PointMass
            (
                x: new AxisStatus(3.4), //Position = 3.4, zero velocity
                y: new AxisStatus(0, 10), //Zero position, velocity = 10
                z: new AxisStatus(1.1, 2), //Position = 1.1, Velocity = 2
                mass: 77.7
            );
            context.AddEntity
            (
                "freeFallEntity", //key
                freeFallEntity, //entity
                c => //Only force that have inpact on this entity
                -new Force 
                (
                    xComponent: 0,
                    yComponent: c["freeFallEntity"].Mass * freeFallAcceleration,
                    zComponent: 0
                )
                //In each perion of time this force is vertical and equal to mass of entity multiplied
                //by free fall acceleration.
            );

            Console.WriteLine($"Start state is \n{context["freeFallEntity"]}");
            //Evaluating the state of context after 1 second.
            context.Tick(timeSpan: 1);
            Console.WriteLine($"\nState of entity after 1 second is \n{context["freeFallEntity"]}");

            //If you want to record some data while context is updatnig, 
            //you may subscribe to OnTick event or better use class derived from ContextObserver.
            var yPositionTracker = new ContextTracker<string, double>
            (
                context,
                c => c["freeFallEntity"].Y.Position
            );
            context.Tick(1);

            //Context tracker implements IReadonlyDictionary.
            Console.WriteLine($"\nOn tick 10345 y position is {yPositionTracker[10345]}");
            Console.WriteLine($"\nOn time 1.27 y position is {yPositionTracker.GetApproximately(1.27)}");
            //Throws exception because tracker has started recording when time is context
            //was already 1.00.
            //Console.WriteLine($"On time 0.4 y position is {yPositionTracker.GetApproximately(0.4)}");
            //Throws exception because tracker hasn't record data yet because time in context is 2.00.
            //Console.WriteLine($"On time 2.7 y position is {yPositionTracker.GetApproximately(2.7)}");

            //Don't forget to dispose tracker when you don't need it anymore.
            //It will increase performance because tracker doesn't record data anymore.
            yPositionTracker.Dispose();
            context.Tick(1);
            //Time is context is 3.0, but tracker is already disposed and doesn't record data anymore.
            Console.WriteLine
            (
                $"\nTracker records data during time span {yPositionTracker.ObservationBeginTime} - {yPositionTracker.LastObservationTime}" +
                $"\nAverage y position is {yPositionTracker.Sum(record => record.Value) / yPositionTracker.Count}"
            );

            //However, if you want to record only max, min, average value etc,
            //better use ContextDependentValue, because it doesn't use a lot of memory
            //to record value in each tick.
            var maxYVelocity = new ContextDependentValue<string, double>
            (
                startValue: context["freeFallEntity"].Y.Velocity,
                observableContext: context,
                newValueFunc: (c, oldValue) => 
                c["freeFallEntity"].Velocity > oldValue ? new double?(c["freeFallEntity"].Velocity) : null
                //null value means that there is no reason to change value
            );

            context.Tick(1);
            Console.WriteLine
            (
                $"\nMax y velocity in time span {maxYVelocity.ObservationBeginTime} - " +
                $"{maxYVelocity.LastObservationTime} is {maxYVelocity.Value}"
            );

            //So, lets create something more complex.
            //What about the spring pendulum with air ressistence?
            var pendulumContext = new PhysicalContext<PendulumEntities>(dt, 2);
            var axis = new PointMass(0, 0, 0, 0);
            var mass = new PointMass(1, 0, 0, 0);
            pendulumContext.AddEntity(PendulumEntities.Axis, axis);
            pendulumContext.AddEntity
            (
                PendulumEntities.Mass,
                mass,
                c => new Force (0, c[PendulumEntities.Mass].Mass * freeFallAcceleration, 0), //Gravity
                HookesLaw.GetLaw //Mechanix.Laws contains amount of static classes to help force evaluating.
                (
                    PendulumEntities.Mass,
                    PendulumEntities.Axis,
                    1, //undeformed lenght of spring
                    10 //elasticity coefficient
                )                
            );

            //Or set of elastic balls.
            var ballsContext = new PhysicalContext<int>(dt, 100);
            var random = new Random();
            const double radius = 1;
            const double elasticity = 100;
            for (int i = 0; i < 100; ++i)
            {
                var ball = new PointMass
                (
                    new AxisStatus(random.NextDouble(), random.NextDouble()),
                    new AxisStatus(random.NextDouble(), random.NextDouble()),
                    new AxisStatus(random.NextDouble(), random.NextDouble()),
                    random.NextDouble()
                );
                ballsContext.AddEntity
                (
                    i,
                    ball,
                    c =>
                    {
                        var force = Force.Zero;
                        foreach (var pair in c)
                        {
                            if (pair.Key != i) force += RadialCollisionLaw.Eval
                            (
                                c[i],
                                pair.Value,
                                radius,
                                elasticity
                            );
                        }
                        return force;
                    }
                );
            }
        }
    }
}
