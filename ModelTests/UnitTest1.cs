using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SpaceWars
{
    [TestClass]
    public class ModelTests
    {

        [TestMethod]
        public void TestSpawnShip()
        {
            World testWorld = new World();
            Assert.IsTrue(testWorld.getPlayers().Count == 0);
            testWorld.SpawnShip(1, "testname");
            Assert.IsTrue(testWorld.getPlayers().Count == 1);
            Assert.IsTrue(testWorld.getPlayers().Contains(1));
        }

        [TestMethod]
        public void TestSpawning6Ships()
        {
            World testWorld = new World();
            Assert.IsTrue(testWorld.getPlayers().Count == 0);
            testWorld.SpawnShip(1, "testname");
            testWorld.SpawnShip(3, "testname");
            testWorld.SpawnShip(5, "testname");
            testWorld.SpawnShip(2, "testname");
            testWorld.SpawnShip(4, "testname");
            testWorld.SpawnShip(6, "testname");
            Assert.IsTrue(testWorld.getPlayers().Count == 6);
            Assert.IsTrue(testWorld.getPlayers().Contains(1));
            Assert.IsTrue(testWorld.getPlayers().Contains(2));
            Assert.IsTrue(testWorld.getPlayers().Contains(3));
            Assert.IsTrue(testWorld.getPlayers().Contains(4));
            Assert.IsTrue(testWorld.getPlayers().Contains(5));
            Assert.IsTrue(testWorld.getPlayers().Contains(6));
        }

        [TestMethod]
        public void TestSpawning10000Ships()
        {
            World testWorld = new World();
            for (int i = 0; i < 10_000; ++i)
            {
                testWorld.SpawnShip(i, "testname");
            }
            Assert.IsTrue(testWorld.getPlayers().Count == 10_000);
        }

        [TestMethod]
        public void TestRemoveShip()
        {
            World testWorld = new World();
            Assert.IsTrue(testWorld.getPlayers().Count == 0);
            testWorld.SpawnShip(1, "testname");
            Assert.IsTrue(testWorld.getPlayers().Count == 1);
            Assert.IsTrue(testWorld.getPlayers().Contains(1));
            testWorld.RemoveShip(1);
            Assert.IsTrue(testWorld.getPlayers().Count == 0);
        }

        [TestMethod]
        public void TestSpawnProjectile()
        {
            World testWorld = new World();
            testWorld.SpawnShip(1, "testname");
            Assert.IsTrue(testWorld.getPlayers().Contains(1));
            Ship testname = (Ship)testWorld.getPlayers()[1];
            Assert.IsTrue(testWorld.getProjs().Count == 0);
            testWorld.SpawnProjectile(testname);
            Assert.IsTrue(testWorld.getProjs().Count == 1);
            Assert.IsTrue(testWorld.getProjs().Contains(0));
        }

        [TestMethod]
        public void TestRemoveProjectile()
        {
            World testWorld = new World();
            testWorld.SpawnShip(1, "testName");
            Ship testShip = (Ship)testWorld.getPlayers()[1];

            Assert.IsTrue(testWorld.getProjs().Count == 0);

            testWorld.SpawnProjectile(testShip);

            Assert.IsTrue(testWorld.getProjs().Count == 1);
            Assert.IsTrue(testWorld.getProjs().Contains(0));

            Projectile testProj = ((Dictionary<int, Projectile>)testWorld.getProjs())[0];

            testWorld.removeProjectile(testProj);

            Assert.IsTrue(testWorld.getProjs().Count == 0);
            Assert.IsFalse(testWorld.getProjs().Contains(0));
        }

        [TestMethod]
        public void TestProjectileVelocity()
        {
            World testWorld = new World();
            testWorld.SpawnShip(1, "testname");
            Ship testShip = (Ship)testWorld.getPlayers()[1];
            Assert.IsTrue(testWorld.getProjs().Count == 0);

            testWorld.SpawnProjectile(testShip);


            Projectile testProj = ((Dictionary<int, Projectile>)testWorld.getProjs())[0];

            double xProj = testProj.GetLocation().GetX();
            double yProj = testProj.GetLocation().GetY();

            double xShip = testShip.GetLocation().GetX();
            double yShip = testShip.GetLocation().GetY();


            Assert.IsTrue(xProj == xShip);
            Assert.IsTrue(yProj == yShip);

            testWorld.updateProjectiles();

            xProj = testProj.GetLocation().GetX();
            yProj = testProj.GetLocation().GetY();

            Assert.IsTrue(xProj == xShip);
            Assert.IsTrue(yProj == yShip);

            testProj.SetVelocity(new Vector2D(1, 2));
            testWorld.updateProjectiles();

            xProj = testProj.GetLocation().GetX();
            yProj = testProj.GetLocation().GetY();

            Assert.IsTrue(xProj == xShip + 1);
            Assert.IsTrue(yProj == yShip + 2);
        }

        [TestMethod]
        public void TestShipVelocityWith1FrameOfThrust()
        {
            World testWorld = new World();
            testWorld.ShipAccel = 0.08;
            testWorld.SpawnShip(1, "testname");
            Ship testShip = (Ship)testWorld.getPlayers()[1];

            double xShipStart = testShip.GetLocation().GetX();
            double yShipStart = testShip.GetLocation().GetY();

            double xShipVel = testShip.GetVelocity().GetX();
            double yShipVel = testShip.GetVelocity().GetY();

            double xShip = testShip.GetLocation().GetX();
            double yShip = testShip.GetLocation().GetY();


            Assert.IsTrue(xShipVel == 0);
            Assert.IsTrue(yShipVel == 0);
            Assert.IsTrue(xShipStart == xShip);
            Assert.IsTrue(yShipStart == yShip);

            testShip.Thrust();

            xShipVel = testShip.GetVelocity().GetX();
            yShipVel = testShip.GetVelocity().GetY();

            Assert.IsTrue(xShipVel == 0);
            Assert.IsTrue(yShipVel == 0.08 * -1);

            testWorld.updateShips();

            xShip = testShip.GetLocation().GetX();
            yShip = testShip.GetLocation().GetY();

            Assert.IsTrue(xShipStart + xShipVel == xShip);
            Assert.IsTrue(yShipStart + yShipVel == yShip);
        }

        [TestMethod]
        public void TestShipVelocityWith5FramesOfThrust()
        {
            World testWorld = new World();
            testWorld.ShipAccel = 0.08;
            testWorld.SpawnShip(1, "testname");
            Ship testShip = (Ship)testWorld.getPlayers()[1];

            double xShipStart = testShip.GetLocation().GetX();
            double yShipStart = testShip.GetLocation().GetY();

            double xShipVel = testShip.GetVelocity().GetX();
            double yShipVel = testShip.GetVelocity().GetY();

            double xShip = testShip.GetLocation().GetX();
            double yShip = testShip.GetLocation().GetY();


            Assert.IsTrue(xShipVel == 0);
            Assert.IsTrue(yShipVel == 0);
            Assert.IsTrue(xShipStart == xShip);
            Assert.IsTrue(yShipStart == yShip);

            for (int i = 0; i < 5; i++)
            {
                testShip.Thrust();
            }

            xShipVel = testShip.GetVelocity().GetX();
            yShipVel = testShip.GetVelocity().GetY();

            Assert.IsTrue(xShipVel == 0);
            Assert.IsTrue(yShipVel == 0.08 * -5);

            testShip.Move();

            xShip = testShip.GetLocation().GetX();
            yShip = testShip.GetLocation().GetY();

            Assert.IsTrue(xShipStart + xShipVel == xShip);
            Assert.IsTrue(yShipStart + yShipVel == yShip);
        }

        [TestMethod]
        public void TestShipVelocityWith100FramesOfThrust()
        {
            World testWorld = new World();
            testWorld.ShipAccel = 0.08;
            testWorld.SpawnShip(1, "testname");
            Ship testShip = (Ship)testWorld.getPlayers()[1];

            double xShipStart = testShip.GetLocation().GetX();
            double yShipStart = testShip.GetLocation().GetY();

            double xShipVel = testShip.GetVelocity().GetX();
            double yShipVel = testShip.GetVelocity().GetY();

            double xShip = testShip.GetLocation().GetX();
            double yShip = testShip.GetLocation().GetY();


            Assert.IsTrue(xShipVel == 0);
            Assert.IsTrue(yShipVel == 0);
            Assert.IsTrue(xShipStart == xShip);
            Assert.IsTrue(yShipStart == yShip);

            for (int i = 0; i < 100; i++)
            {
                testShip.Thrust();
            }

            xShipVel = testShip.GetVelocity().GetX();
            yShipVel = testShip.GetVelocity().GetY();

            Assert.IsTrue(xShipVel == 0);
            Assert.AreEqual(yShipVel, 0.08 * -100, 0.000001);

            testShip.Move();

            xShip = testShip.GetLocation().GetX();
            yShip = testShip.GetLocation().GetY();

            Assert.IsTrue(xShipStart + xShipVel == xShip);
            Assert.IsTrue(yShipStart + yShipVel == yShip);
        }

        [TestMethod]
        public void TestShipVelocityWith10000FramesOfThrust()
        {
            World testWorld = new World();
            testWorld.ShipAccel = 0.08;
            testWorld.SpawnShip(1, "testname");
            Ship testShip = (Ship)testWorld.getPlayers()[1];

            double xShipStart = testShip.GetLocation().GetX();
            double yShipStart = testShip.GetLocation().GetY();

            double xShipVel = testShip.GetVelocity().GetX();
            double yShipVel = testShip.GetVelocity().GetY();

            double xShip = testShip.GetLocation().GetX();
            double yShip = testShip.GetLocation().GetY();


            Assert.IsTrue(xShipVel == 0);
            Assert.IsTrue(yShipVel == 0);
            Assert.IsTrue(xShipStart == xShip);
            Assert.IsTrue(yShipStart == yShip);

            for (int i = 0; i < 10000; i++)
            {
                testShip.Thrust();
            }

            xShipVel = testShip.GetVelocity().GetX();
            yShipVel = testShip.GetVelocity().GetY();

            Assert.IsTrue(xShipVel == 0);
            Assert.AreEqual(yShipVel, 0.08 * -10000, 0.000001);
        }

        [TestMethod]
        public void TestApplyStarGravity()
        {
            World testWorld = new World();
            testWorld.SpawnShip(1, "testname");
            testWorld.SpawnStars(0, 0, 0.1);

            testWorld.ApplyStarGravity();
        }

        [TestMethod]
        public void TestEnableMarioMode()
        {
            World testWorld = new World();

            testWorld.EnableMarioMode(true);
            Assert.IsTrue(testWorld.GetMarioMode());
        }

        [TestMethod]
        public void TestGetCurrentPlayerWithoutDeath()
        {
            World testWorld = new World();
            Dictionary<int, Ship> test;

            testWorld.SpawnShip(1, "testname1");
            testWorld.SpawnShip(3, "testname2");
            testWorld.SpawnShip(5, "testname3");
            testWorld.SpawnShip(2, "testname4");
            testWorld.SpawnShip(4, "testname5");
            testWorld.SpawnShip(6, "testname6");

            test = (Dictionary<int, Ship>)testWorld.getPlayers();
            Assert.IsTrue(test.ContainsKey(1));
            Assert.IsTrue(test.ContainsKey(2));
            Assert.IsTrue(test.ContainsKey(3));
            Assert.IsTrue(test.ContainsKey(4));
            Assert.IsTrue(test.ContainsKey(5));
            Assert.IsTrue(test.ContainsKey(6));
        }

        [TestMethod]
        public void TestGetCurrentPlayerWithRemoval()
        {
            World testWorld = new World();
            Dictionary<int, Ship> test;

            testWorld.SpawnShip(1, "testname");
            testWorld.SpawnShip(3, "testname");
            testWorld.SpawnShip(5, "testname");
            testWorld.SpawnShip(2, "testname");
            testWorld.SpawnShip(4, "testname");
            testWorld.SpawnShip(6, "testname");

            testWorld.RemoveShip(1);

            test = (Dictionary<int, Ship>)testWorld.getPlayers();
            Assert.IsFalse(test.ContainsKey(1));
            Assert.IsTrue(test.ContainsKey(2));
            Assert.IsTrue(test.ContainsKey(3));
            Assert.IsTrue(test.ContainsKey(4));
            Assert.IsTrue(test.ContainsKey(5));
            Assert.IsTrue(test.ContainsKey(6));
        }

        [TestMethod]
        public void TestGetDeadPlayers()
        {
            World testWorld = new World();
            Dictionary<int, Ship> testAlive;
            Dictionary<int, Ship> testDead;

            testWorld.SpawnShip(1, "testname");
            testWorld.SpawnShip(3, "testname");
            testWorld.SpawnShip(5, "testname");
            testWorld.SpawnShip(2, "testname");
            testWorld.SpawnShip(4, "testname");
            testWorld.SpawnShip(6, "testname");

            Ship deadShip1 = (Ship)testWorld.getPlayers()[1];
            Ship deadShip2 = (Ship)testWorld.getPlayers()[2];
            testWorld.KillShip(deadShip1);
            testWorld.KillShip(deadShip2);

            testAlive = (Dictionary<int, Ship>)testWorld.getPlayers();
            Assert.IsFalse(testAlive.ContainsKey(1));
            Assert.IsFalse(testAlive.ContainsKey(2));
            Assert.IsTrue(testAlive.ContainsKey(3));
            Assert.IsTrue(testAlive.ContainsKey(4));
            Assert.IsTrue(testAlive.ContainsKey(5));
            Assert.IsTrue(testAlive.ContainsKey(6));

            testDead = (Dictionary<int, Ship>)testWorld.GetDeadPlayers();
            Assert.IsTrue(testDead.ContainsKey(1));
            Assert.IsTrue(testDead.ContainsKey(2));
            Assert.IsFalse(testDead.ContainsKey(3));
            Assert.IsFalse(testDead.ContainsKey(4));
            Assert.IsFalse(testDead.ContainsKey(5));
            Assert.IsFalse(testDead.ContainsKey(6));
        }

        [TestMethod]
        public void TestUpdatingShipLocationInWorldWithNewShip()
        {
            World testWorld = new World();

            testWorld.SpawnShip(1, "testName");
            Ship firstFrame = (Ship)testWorld.getPlayers()[1];

            double xShipFirstFrame = firstFrame.GetLocation().GetX();
            double yShipFirstFrame = firstFrame.GetLocation().GetY();

            Ship secondFrame = new Ship(1, firstFrame.GetLocation() + firstFrame.GetLocation(), firstFrame.GetOrientation(), false, "testName", 5, 0);

            testWorld.addShip(secondFrame);

            Assert.IsTrue(testWorld.getPlayers().Count == 1);

            Ship sameShip = (Ship)testWorld.getPlayers()[1];
            double xShipSecondFrame = sameShip.GetLocation().GetX();
            double yShipSecondFrame = sameShip.GetLocation().GetY();
            Assert.IsTrue(xShipFirstFrame != xShipSecondFrame);
            Assert.IsTrue(yShipFirstFrame != yShipSecondFrame);

        }

        [TestMethod]
        public void TestUpdatingStarLocationInWorldWithNewStar()
        {
            World testWorld = new World();
            Vector2D newStarLoc = new Vector2D(50, 50);

            testWorld.SpawnStars(0, 0, 1);
            Star firstFrame = (Star)testWorld.getStars()[0];

            double xStarFirstFrame = firstFrame.GetLocation().GetX();
            double yStarFirstFrame = firstFrame.GetLocation().GetY();

            Star secondFrame = new Star(0, newStarLoc, 1);

            testWorld.addStar(secondFrame);

            Assert.IsTrue(testWorld.getStars().Count == 1);

            Star sameStar = (Star)testWorld.getStars()[0];
            double xStarSecondFrame = sameStar.GetLocation().GetX();
            double yStarSecondFrame = sameStar.GetLocation().GetY();
            Assert.IsTrue(xStarFirstFrame != xStarSecondFrame);
            Assert.IsTrue(yStarFirstFrame != yStarSecondFrame);

        }

        [TestMethod]
        public void TestGettingAndSettingWorldSettings()
        {
            World testWorld = new World();

            testWorld.SetUniverseSize(750);
            Assert.AreEqual(testWorld.GetWorldSize(), 750);

            testWorld.setPlayerID(1);
            Assert.AreEqual(testWorld.GetCurrentPlayer(), 1);

            testWorld.BossHealth = 100;
            Assert.AreEqual(testWorld.BossHealth, 100);

            testWorld.SetFireDelay(10);
            Assert.AreEqual(testWorld.FireDelay, 10);
            testWorld.FireDelay = 20;
            Assert.AreEqual(testWorld.FireDelay, 20);

            testWorld.ProjVelocity = 30;
            Assert.AreEqual(testWorld.ProjVelocity, 30);

            testWorld.ShipHealth = 6;
            Assert.AreEqual(testWorld.ShipHealth, 6);

            testWorld.ShipRespawnRate = 210;
            Assert.AreEqual(testWorld.ShipRespawnRate, 210);

            testWorld.ShipSize = 840;
            Assert.AreEqual(testWorld.ShipSize, 840);

            testWorld.ShipTurn = 3;
            Assert.AreEqual(testWorld.ShipTurn, 3);

            testWorld.StarSize = 421;
            Assert.AreEqual(testWorld.StarSize, 421);
        }

        [TestMethod]
        public void TestHasCollidedProjStar()
        {
            World testWorld = new World();
            Vector2D collisionPoint = new Vector2D(0, 0);
            Vector2D noCollisionPoint = new Vector2D(50, 50);

            Star star = new Star(0, collisionPoint, 1);
            testWorld.addStar(star);

            Projectile projColliding = new Projectile(1, collisionPoint, collisionPoint, true, 1);
            Projectile projNotColliding = new Projectile(1, noCollisionPoint, noCollisionPoint, true, 1);
            testWorld.addProjectile(projColliding);
            testWorld.addProjectile(projNotColliding);

            Assert.IsTrue(testWorld.HasCollidedProjStar(star, projColliding));
            Assert.IsFalse(testWorld.HasCollidedProjStar(star, projNotColliding));
        }

        [TestMethod]
        public void TestHasCollidedShipStar()
        {
            World testWorld = new World();
            Vector2D collisionPoint = new Vector2D(0, 0);
            Vector2D noCollisionPoint = new Vector2D(50, 50);

            Ship test1 = new Ship(1, collisionPoint, collisionPoint, false, "testName", 5, 0);
            Ship test2 = new Ship(2, noCollisionPoint, noCollisionPoint, false, "testName", 5, 0);
            testWorld.addShip(test1);
            testWorld.addShip(test2);

            Star star = new Star(0, collisionPoint, 1);
            testWorld.addStar(star);

            Assert.IsTrue(testWorld.HasCollidedShipStar(test1, star));
            Assert.IsFalse(testWorld.HasCollidedShipStar(test2, star));
        }

        [TestMethod]
        public void TestHasCollidedShipProj()
        {
            World testWorld = new World();
            Vector2D ship1Loc = new Vector2D(0, 0);
            Vector2D ship2Loc = new Vector2D(50, 50);
            Vector2D missedProjLoc = new Vector2D(-50, -50);

            Ship test1 = new Ship(1, ship1Loc, ship1Loc, false, "testName", 5, 0);
            Ship test2 = new Ship(2, ship2Loc, ship2Loc, false, "testName", 5, 0);
            testWorld.addShip(test1);
            testWorld.addShip(test2);

            Projectile projColliding = new Projectile(1, ship1Loc, ship1Loc, true, 2);
            Projectile projNotColliding = new Projectile(1, missedProjLoc, missedProjLoc, true, 2);
            Projectile projPassingOwner = new Projectile(1, ship2Loc, ship2Loc, true, 2);
            testWorld.addProjectile(projColliding);
            testWorld.addProjectile(projNotColliding);
            testWorld.addProjectile(projPassingOwner);

            Assert.IsTrue(testWorld.HasCollidedShipProj(test1, projColliding));
            Assert.IsFalse(testWorld.HasCollidedShipProj(test1, projNotColliding));
            Assert.IsFalse(testWorld.HasCollidedShipProj(test2, projPassingOwner));
        }

        [TestMethod]
        public void TestShipRespawningAndRespawnCounter()
        {
            World testWorld = new World();
            testWorld.ShipRespawnRate = 300;
            Vector2D shipLoc = new Vector2D(0, 0);

            Ship test1 = new Ship(1, shipLoc, shipLoc, false, "testName", 5, 0);
            testWorld.SpawnShip(2, "testname2");
            testWorld.addShip(test1);

            Assert.IsTrue(testWorld.getPlayers().Count == 2);

            testWorld.KillShip(test1);

            Assert.IsTrue(testWorld.getPlayers().Count == 1);

            for (int i = 0; i < 299; ++i)
            {
                Assert.IsTrue(testWorld.getPlayers().Count == 1);

                testWorld.updateRespawnCounter();
            }

            Assert.IsTrue(testWorld.getPlayers().Count == 2);
        }

        [TestMethod]
        public void TestStarRespawningAndRespawnCounter()
        {
            World testWorld = new World();
            testWorld.ShipRespawnRate = 300;
            Vector2D starLoc = new Vector2D(0, 0);

            Star star = new Star(1, starLoc, 1);
            testWorld.SpawnStars(50, 50, 2);
            testWorld.addStar(star);

            Assert.IsTrue(testWorld.getStars().Count == 2);

            testWorld.KillStar(star);

            Assert.IsTrue(testWorld.GetDeadStars().Count == 1);

            Assert.IsTrue(testWorld.getStars().Count == 1);

            for (int i = 0; i < 299; ++i)
            {
                Assert.IsTrue(testWorld.getStars().Count == 1);

                testWorld.updateRespawnCounter();
            }

            Assert.IsTrue(testWorld.getStars().Count == 2);
        }

        [TestMethod]
        public void TestBossModeFunctionality()
        {
            World testWorld = new World();
            Vector2D starLoc = new Vector2D(0, 0);

            Star star = new Star(1, starLoc, 1);
            testWorld.addStar(star);
            testWorld.SpawnStars(0, 0, 1);

            for (int i = 0; i < 10; i++)
            {
                testWorld.bossFireMode1(star);
            }
            Assert.IsTrue(testWorld.getProjs().Count == 5);

            for (int i = 0; i < 18; i++)
            {
                testWorld.bossFireMode2(star);
            }
            Assert.IsTrue(testWorld.getProjs().Count == (5 + (3 * 4)));

            for (int i = 0; i < 1000; i++)
            {
                testWorld.bossFireMode3(star);
            }
            Assert.IsTrue(testWorld.getProjs().Count == (5 + (3 * 4) + 1000));
        }

        [TestMethod]
        public void TestShipProcessingCommands()
        {
            World testWorld = new World();
            Vector2D shipLoc = new Vector2D(0, 0);

            Ship test1 = new Ship(1, shipLoc, shipLoc, false, "testName", 5, 0);

            test1.queueCommands("TL");
            Assert.AreEqual("TL", test1.GetCommands());
            test1.ProcessCommands();
            test1.queueCommands("RT");
            Assert.AreEqual("RT", test1.GetCommands());
            test1.ProcessCommands();
            test1.queueCommands("F");
            Assert.AreEqual("F", test1.GetCommands());
            test1.ProcessCommands();

            testWorld.updateShips();
        }
    }
}