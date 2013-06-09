using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories
{
    /// <summary>
    /// An easy to use factory for using joints.
    /// </summary>
    public static class JointFactory
    {
        #region Revolute Joint

        /// <summary>
        /// Creates a revolute joint.
        /// </summary>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localAnchorB">The anchor of bodyB in local coordinates</param>
        /// <returns></returns>
        public static FSRevoluteJoint CreateRevoluteJoint(FSBody bodyA, FSBody bodyB, FVector2 localAnchorB)
        {
            FVector2 localanchorA = bodyA.GetLocalPoint(bodyB.GetWorldPoint(localAnchorB));
            FSRevoluteJoint joint = new FSRevoluteJoint(bodyA, bodyB, localanchorA, localAnchorB);
            return joint;
        }

        /// <summary>
        /// Creates a revolute joint and adds it to the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public static FSRevoluteJoint CreateRevoluteJoint(FSWorld world, FSBody bodyA, FSBody bodyB, FVector2 anchor)
        {
            FSRevoluteJoint joint = CreateRevoluteJoint(bodyA, bodyB, anchor);
            world.AddJoint(joint);
            return joint;
        }

        ///// <summary>
        ///// Creates the fixed revolute joint.
        ///// </summary>
        ///// <param name="world">The world.</param>
        ///// <param name="body">The body.</param>
        ///// <param name="bodyAnchor">The body anchor.</param>
        ///// <param name="worldAnchor">The world anchor.</param>
        ///// <returns></returns>
        //public static FixedRevoluteJoint CreateFixedRevoluteJoint(World world, Body body, Vector2 bodyAnchor,
        //                                                          Vector2 worldAnchor)
        //{
        //    FixedRevoluteJoint fixedRevoluteJoint = new FixedRevoluteJoint(body, bodyAnchor, worldAnchor);
        //    world.AddJoint(fixedRevoluteJoint);
        //    return fixedRevoluteJoint;
        //}

        #endregion

        #region Weld Joint

        /// <summary>
        /// Creates a weld joint
        /// </summary>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
		/// <param name="worldAnchor">World space coordinates of weld joint</param>
        /// <returns></returns>
		public static FSWeldJoint CreateWeldJoint(FSBody bodyA, FSBody bodyB, FVector2 worldAnchor)
        {
			FSWeldJoint joint = new FSWeldJoint(bodyA, bodyB, bodyA.GetLocalPoint(worldAnchor),
											bodyB.GetLocalPoint(worldAnchor));
            return joint;
        }

        /// <summary>
        /// Creates a weld joint and adds it to the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
		/// <param name="worldAnchor">World space coordinates of weld joint</param>
        /// <returns></returns>
        public static FSWeldJoint CreateWeldJoint(FSWorld world, FSBody bodyA, FSBody bodyB, FVector2 worldAnchor)
        {
			FSWeldJoint joint = CreateWeldJoint(bodyA, bodyB, worldAnchor);
            world.AddJoint(joint);
            return joint;
        }

        public static FSWeldJoint CreateWeldJoint(FSWorld world, FSBody bodyA, FSBody bodyB, FVector2 localAnchorA,
                                                FVector2 localAnchorB)
        {
            FSWeldJoint weldJoint = new FSWeldJoint(bodyA, bodyB, localAnchorA, localAnchorB);
            world.AddJoint(weldJoint);
            return weldJoint;
        }

        #endregion

        #region Prismatic Joint

        /// <summary>
        /// Creates a prsimatic joint
        /// </summary>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorB"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static FSPrismaticJoint CreatePrismaticJoint(FSBody bodyA, FSBody bodyB, FVector2 localanchorB, FVector2 axis)
        {
            FVector2 localanchorA = bodyA.GetLocalPoint(bodyB.GetWorldPoint(localanchorB));
            FSPrismaticJoint joint = new FSPrismaticJoint(bodyA, bodyB, localanchorA, localanchorB, axis);
            return joint;
        }

        /// <summary>
        /// Creates a prismatic joint and adds it to the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorB"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static FSPrismaticJoint CreatePrismaticJoint(FSWorld world, FSBody bodyA, FSBody bodyB, FVector2 localanchorB,
                                                          FVector2 axis)
        {
            FSPrismaticJoint joint = CreatePrismaticJoint(bodyA, bodyB, localanchorB, axis);
            world.AddJoint(joint);
            return joint;
        }

        //public static FixedPrismaticJoint CreateFixedPrismaticJoint(World world, Body body, Vector2 worldAnchor,
        //                                                            Vector2 axis)
        //{
        //    FixedPrismaticJoint joint = new FixedPrismaticJoint(body, worldAnchor, axis);
        //    world.AddJoint(joint);
        //    return joint;
        //}

        #endregion

        #region Wheel Joint

        /// <summary>
        /// Creates a Wheel Joint
        /// </summary>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="anchor"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static FSWheelJoint CreateWheelJoint(FSBody bodyA, FSBody bodyB, FVector2 anchor, FVector2 axis)
        {
            FSWheelJoint joint = new FSWheelJoint(bodyA, bodyB, anchor, axis);
            return joint;
        }

        /// <summary>
        /// Creates a Wheel Joint and adds it to the world
        /// </summary>
        /// <param name="world"></param>
        /// <param name="bodyA"></param>
        /// <param name="bodyB"></param>
        /// <param name="localanchorB"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static FSWheelJoint CreateWheelJoint(FSWorld world, FSBody bodyA, FSBody bodyB, FVector2 localanchorB, FVector2 axis)
        {
            FSWheelJoint joint = CreateWheelJoint(bodyA, bodyB, localanchorB, axis);
            world.AddJoint(joint);
            return joint;
        }

        #endregion

        #region Angle Joint

        /// <summary>
        /// Creates an angle joint.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="bodyA">The first body.</param>
        /// <param name="bodyB">The second body.</param>
        /// <returns></returns>
        public static FSAngleJoint CreateAngleJoint(FSWorld world, FSBody bodyA, FSBody bodyB)
        {
            FSAngleJoint angleJoint = new FSAngleJoint(bodyA, bodyB);
            world.AddJoint(angleJoint);

            return angleJoint;
        }

        /// <summary>
        /// Creates a fixed angle joint.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="body">The body.</param>
        /// <returns></returns>
        public static FSFixedAngleJoint CreateFixedAngleJoint(FSWorld world, FSBody body)
        {
            FSFixedAngleJoint angleJoint = new FSFixedAngleJoint(body);
            world.AddJoint(angleJoint);

            return angleJoint;
        }

        #endregion

        #region Distance Joint

        public static FSDistanceJoint CreateDistanceJoint(FSWorld world, FSBody bodyA, FSBody bodyB, FVector2 anchorA,
                                                        FVector2 anchorB)
        {
            FSDistanceJoint distanceJoint = new FSDistanceJoint(bodyA, bodyB, anchorA, anchorB);
            world.AddJoint(distanceJoint);
            return distanceJoint;
        }

        //public static FixedDistanceJoint CreateFixedDistanceJoint(World world, Body body, Vector2 localAnchor,
        //                                                          Vector2 worldAnchor)
        //{
        //    FixedDistanceJoint distanceJoint = new FixedDistanceJoint(body, localAnchor, worldAnchor);
        //    world.AddJoint(distanceJoint);
        //    return distanceJoint;
        //}

        #endregion

        #region Friction Joint

        public static FSFrictionJoint CreateFrictionJoint(FSWorld world, FSBody bodyA, FSBody bodyB, FVector2 anchorA,
                                                        FVector2 anchorB)
        {
            FSFrictionJoint frictionJoint = new FSFrictionJoint(bodyA, bodyB, anchorA, anchorB);
            world.AddJoint(frictionJoint);
            return frictionJoint;
        }

        //public static FixedFrictionJoint CreateFixedFrictionJoint(World world, Body body, Vector2 bodyAnchor)
        //{
        //    FixedFrictionJoint frictionJoint = new FixedFrictionJoint(body, bodyAnchor);
        //    world.AddJoint(frictionJoint);
        //    return frictionJoint;
        //}

        #endregion

        #region Gear Joint

        public static FSGearJoint CreateGearJoint(FSWorld world, FarseerJoint jointA, FarseerJoint jointB, float ratio)
        {
            FSGearJoint gearJoint = new FSGearJoint(jointA, jointB, ratio);
            world.AddJoint(gearJoint);
            return gearJoint;
        }

        #endregion

        #region Pulley Joint

        public static FSPulleyJoint CreatePulleyJoint(FSWorld world, FSBody bodyA, FSBody bodyB, FVector2 groundAnchorA,
                                                    FVector2 groundAnchorB, FVector2 anchorA, FVector2 anchorB, float ratio)
        {
            FSPulleyJoint pulleyJoint = new FSPulleyJoint(bodyA, bodyB, groundAnchorA, groundAnchorB, anchorA, anchorB,
                                                      ratio);
            world.AddJoint(pulleyJoint);
            return pulleyJoint;
        }

        #endregion

        #region Slider Joint

        public static FSSliderJoint CreateSliderJoint(FSWorld world, FSBody bodyA, FSBody bodyB, FVector2 anchorA,
                                                    FVector2 anchorB, float minLength, float maxLength)
        {
            FSSliderJoint sliderJoint = new FSSliderJoint(bodyA, bodyB, anchorA, anchorB, minLength, maxLength);
            world.AddJoint(sliderJoint);
            return sliderJoint;
        }

        #endregion

        public static FSFixedMouseJoint CreateFixedMouseJoint(FSWorld world, FSBody body, FVector2 target)
        {
            FSFixedMouseJoint joint = new FSFixedMouseJoint(body, target);
            world.AddJoint(joint);
            return joint;
        }
    }
}