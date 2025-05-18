using Moq;
using System.Drawing;
using GameMechanics.Model;
using GameMechanics.Persistence;

namespace GameMechanicsMsTest {
    [TestClass]
    public class Test {

        private GameModel? _game = null;

        [TestInitialize]
        public void InitGameMechanicsTest() {
            _game = new GameModel(new GameFileDataAccess());
        }

        [TestMethod]
        public async Task LoadTestAsync() {
            /*
            6
            2,2,2,1,1,1
            2,2,2,1,1,1
            2,2,2,1,1,1
            2,2,2,1,1,1
            0,0,0,0,0,0
            0,0,0,0,0,0
            12,Gold,gold
            12,Cyan,cyan
             */


            Assert.IsNotNull(_game);
            await _game.LoadGameAsync("test6x6.txt");

            Assert.AreEqual(6, _game?.TableSize);
            CollectionAssert.AreEqual(new int[] { 2, 2, 2, 1, 1, 1 }, _game?._field[0]);
            CollectionAssert.AreEqual(new int[] { 2, 2, 2, 1, 1, 1 }, _game?._field[1]);
            CollectionAssert.AreEqual(new int[] { 2, 2, 2, 1, 1, 1 }, _game?._field[2]);
            CollectionAssert.AreEqual(new int[] { 2, 2, 2, 1, 1, 1 }, _game?._field[3]);
            CollectionAssert.AreEqual(new int[] { 0, 0, 0, 0, 0, 0 }, _game?._field[4]);
            CollectionAssert.AreEqual(new int[] { 0, 0, 0, 0, 0, 0 }, _game?._field[5]);

            Assert.AreEqual(2, _game?.Players.Count);
            Assert.AreEqual("gold", _game?.Players[0].Name);
            Assert.AreEqual("cyan", _game?.Players[1].Name);
            Assert.AreEqual(Color.Gold, _game?.Players[0].Color);
            Assert.AreEqual(Color.Cyan, _game?.Players[1].Color);
            Assert.IsTrue(_game?.Players.Where(player => player.Points == 12).Count() == 2);
        }

        [TestMethod]
        public async Task SaveGameTest() {
            string path = "testSave.txt";
            String[][] players = {
                ["Player1", "red"],
                ["Player2", "blue"]
            };

            _game?.NewGame(3, players[0][0], players[0][1],
                players[1][0], players[1][1]);
            // Act
            Assert.IsNotNull(_game);
            await _game.SaveGameAsync(path);

            // Assert
            Assert.IsTrue(File.Exists(path));
            var savedContent = File.ReadAllText(path).Split('\n');
            Assert.AreEqual("3", savedContent[0]);
            Assert.AreEqual("0,0,0", savedContent[1]);
            Assert.AreEqual("0,0,0", savedContent[2]);
            Assert.AreEqual("0,0,0", savedContent[3]);
            Assert.AreEqual("0,Red,Player1", savedContent[4]);
            Assert.AreEqual("0,Blue,Player2", savedContent[5]);
        }

        [TestMethod]
        public void ClearTest() {

            String[][] players = {
                ["Sanyika","gold"],
                ["Petike","green"]
            };
            Assert.IsNotNull(_game);
            _game.NewGame(3, players[0][0], players[0][1],
                players[1][0], players[1][1]);
            _game.Step(0, 0);
            _game.Step(0, 1);

            _game.NewGame(3);

            // Assert
            foreach (var row in _game._field) {
                CollectionAssert.AreEqual(new int[] { 0, 0, 0 }, row);
            }
            Assert.AreEqual(0, _game.Players[0].Points);
            Assert.AreEqual(0, _game.Players[1].Points);
        }

        [TestMethod]
        public void StepTest() {
            String[][] players = {
                ["Player1", "red"],
                ["Player2", "blue"]
            };
            Assert.IsNotNull(_game);
            _game.NewGame(3, players[0][0], players[0][1],
                players[1][0], players[1][1]);

            _game.Step(0, 0);
            _game.Step(0, 1);

            Assert.AreEqual(1, _game._field[0][0]);
            Assert.AreEqual(1, _game._field[0][1]);
            Assert.AreEqual(2, _game.Players[0].Points);
            Assert.AreEqual(0, _game.Players[1].Points);
        }

        [TestMethod]
        public void EndGameTest() {
            String[][] players = {
                ["Player1", "red"],
                ["Player2", "blue"]
            };
            Assert.IsNotNull(_game);
            _game.NewGame(3, players[0][0], players[0][1],
                players[1][0], players[1][1]);

            _game.Step(0, 1);
            _game.Step(0, 2);
            Assert.ThrowsException<ArgumentException>(() => {
                _game.Step(0, 2);
                _game.Step(1, 0);
            });

            _game.Step(1, 0);
            _game.Step(1, 1);

            bool isGameEnded = _game.EndGame();

            Assert.IsFalse(isGameEnded);

            _game.Step(2, 1);
            _game.Step(2, 2);
            isGameEnded = _game.EndGame();
            Assert.IsTrue(isGameEnded);
        }

        [TestMethod]
        public async Task WinnerTest() {

            Assert.IsNotNull(_game);
            string winner = "";
            _game.GameEnded += (_, gameWinner) => winner = gameWinner;
            await _game.LoadGameAsync("testWin.txt");

            Assert.AreEqual("gold", winner);
        }

        [TestMethod]
        public async Task TestFilling() {

            Assert.IsNotNull(_game);
            await _game.LoadGameAsync("testFilling.txt");
            /*Table now:
             1,1,2,2,2,2,2,2
             0,0,2,2,2,2,2,2
             0,0,2,2,2,2,2,2
             0,0,2,2,2,2,2,2
             1,1,1,2,1,1,1,1
             1,1,1,2,0,0,0,1
             1,1,1,0,0,0,0,1
             1,1,1,0,1,1,1,1
            */

            _game.Step(5, 4);
            _game.Step(6, 4);
            /*Table now:
             1,1,2,2,2,2,2,2
             0,0,2,2,2,2,2,2
             0,0,2,2,2,2,2,2
             0,0,2,2,2,2,2,2
             1,1,1,2,1,1,1,1
             1,1,1,2,1,1,1,1
             1,1,1,0,1,1,1,1
             1,1,1,0,1,1,1,1
            */
            CollectionAssert.AreEqual(new int[] { 1, 1, 1, 2, 1, 1, 1, 1 }, 
                _game._field[5]);
            CollectionAssert.AreEqual(new int[] { 1, 1, 1, 0, 1, 1, 1, 1 }, 
                _game._field[6]);
        }


        [TestMethod]
        [ExpectedException(typeof(System.IO.IOException))]
        public async Task LoadTest_IOException() {
            Assert.IsNotNull(_game);
            await _game.LoadGameAsync("ThisFileDoesNotExist");
        }

        [TestMethod]
        [ExpectedException(typeof(System.IO.IOException))]
        public async Task LoadTestInvalidFileFormat() {
            /*
             "NotAnInteger"
             "0,1,0"
             "1,Red,Player1"
             */
            Assert.IsNotNull(_game);
            await _game.LoadGameAsync("invalidFormat.txt");
        }

        [TestMethod]
        public void TestEventsOnCreation() {
            String[][] players = {
                ["Player1", "red"],
                ["Player2", "blue"]
            };
            bool IsGameReadyInvoked = false;
            bool IsTableChangedInvoked = false;
            bool IsPlayerChangedInvoked = false;
            bool IsPointsChangedInvoked = false;
            Assert.IsNotNull(_game);
            _game.TableReady += (_, _) => IsGameReadyInvoked = true;
            _game.TableChanged += (_, _) => IsTableChangedInvoked = true;
            _game.PlayerChanged += (_, _) => IsPlayerChangedInvoked = true;
            _game.PointsChanged += (_, _) => IsPointsChangedInvoked = true;

            Assert.IsFalse(IsGameReadyInvoked);
            Assert.IsFalse(IsTableChangedInvoked);
            Assert.IsFalse(IsPlayerChangedInvoked);
            Assert.IsFalse(IsPointsChangedInvoked);
            _game.NewGame(10, players[0][0], players[0][1],
                players[1][0], players[1][1]);
            Assert.IsTrue(IsGameReadyInvoked);
            Assert.IsTrue(IsTableChangedInvoked);
            Assert.IsTrue(IsPlayerChangedInvoked);
            Assert.IsTrue(IsPointsChangedInvoked);
        }

        //Can't test because NuGet
        /*[TestMethod]
        [ExpectedException(typeof(IOException))]
        public void LoadTest_NullFileManager_ThrowsIOException() {
            IFileManager ?fileManager = null;
            Assert.IsNull(fileManager);
            _game = new Game(fileManager);
        }*/

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Step_OutOfBounds_ThrowsIndexOutOfRangeException() {
            String[][] players = {
                ["Player1", "red"],
                ["Player2", "blue"]
            };
            Assert.IsNotNull(_game);

            _game.NewGame(3, players[0][0], players[0][1],
                players[1][0], players[1][1]);

            _game.Step(3, 3);
            _game.Step(3, 4);
        }




    }
}