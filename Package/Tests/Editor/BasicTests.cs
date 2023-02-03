using NUnit.Framework;

namespace Tests.Editor {
    public class BasicTests {
        [Test]
        public void ChangeProperty() {
            var player = new Data.Player();
            Assert.IsTrue(!player.Alive);
            player.Alive = true;
            Assert.IsTrue(player.Alive);
        }
        
        [Test]
        public void ChangePropertyInitializer() {
            var player = new Data.Player {
                Alive = true
            };
            Assert.IsTrue(player.Alive);
        }
        
        [Test]
        public void ComplexChangeProperty() {
            var player = new Data.Player();
            player.Alive = true;
            Assert.IsTrue(player.Alive);
            player.Team = 2;
            Assert.IsTrue(player.Team == 2);
            player.Alive = false;
            Assert.IsTrue(!player.Alive);
            Assert.IsTrue(player.Team == 2);
        }
    }
}
