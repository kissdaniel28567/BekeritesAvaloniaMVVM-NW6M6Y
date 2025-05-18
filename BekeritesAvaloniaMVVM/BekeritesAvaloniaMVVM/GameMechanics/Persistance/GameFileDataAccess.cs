using GameMechanics.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMechanics.Persistence {
    public class GameFileDataAccess : IGameAccess {

        public async Task<(int[][], List<Player>)> LoadAsync(string path) {
            try {
                String[] loadedGame = await File.ReadAllLinesAsync(path);

                int[][] field;

                int size = int.Parse(loadedGame[0]);
                field = new int[size][];
                for (int i = 0; i < size; i++) {
                    field[i] = loadedGame[i + 1].Split(',').Select(num => int.Parse(num)).ToArray();
                }
                //OnTableReady();
                //Mivel lesz még egy plusz enter utána!!
                List<Player> players = new List<Player>();
                for (int i = size + 1; i < loadedGame.Length; i++) {
                    String[] currentPlayer = loadedGame[i].Split(',');
                    players.Add(new Player(int.Parse(currentPlayer[0]), Color.FromName(currentPlayer[1]), currentPlayer[2]));
                }
                if(players.Count == 1) throw new Exception();
                return (field, players);
                //_isNuget = false;
                //OnTableChanged();
            } catch (Exception) {
                throw new IOException();
            }
        }

        public async Task<(int[][], List<Player>)> LoadAsync(Stream stream) {
            try {
                using (StreamReader? reader = new StreamReader(stream)) {
                    int[][] field;

                    //List<String> example = new List<string>();
                    //example.Add("szia");
                    //example.Add("Viktor");
                    //List<String> example1 = example.Where(word => word == "Viktor").ToList();
                    //List<Char> example2 = example.Select(word => word[0]).ToList();


                    int size = int.Parse(await reader.ReadLineAsync() ?? "");
                    field = new int[size][];
                    for (int i = 0; i < size; i++) {
                        String line = await reader.ReadLineAsync() ?? "";
                        field[i] = line.Split(',').Select(num => int.Parse(num)).ToArray();
                    }
                    //OnTableReady();
                    //Mivel lesz még egy plusz enter utána!!
                    List<Player> players = new List<Player>();
                    for (int i = 0; i < 2; i++) {
                        String line = await reader.ReadLineAsync() ?? "";
                        String[] currentPlayer = line.Split(',');
                        players.Add(new Player(int.Parse(currentPlayer[0]), Color.FromName(currentPlayer[1]), currentPlayer[2]));
                    }
                    if (players.Count == 1) throw new Exception();
                    return (field, players);
                }


            } catch (Exception) {
                throw new IOException("Something went wrong reading the file");
            }
        }

        public async Task SaveAsync(string path, int[][] field, List<Player> players) {
            if (File.Exists(path)) {
                File.Delete(path);
            }

            await File.AppendAllTextAsync(path, field.GetLength(0) + "\n");
            for (int i = 0; i < field.GetLength(0); i++) {
                if (field[i] == null) {
                    throw new NullReferenceException();
                } else {
                    await File.AppendAllTextAsync(path, String.Join(",", field[i]) + "\n");
                }
            }
            //playerek beirasa
            foreach (Player player in players) {
                await File.AppendAllTextAsync(path, player.ToString() + "\n");
            }
        }

        public async Task SaveAsync(Stream stream, int[][] field, List<Player> players) {
            try {
                using (StreamWriter writer = new StreamWriter(stream)) {
                    //if (File.Exists(path)) {
                    //    File.Delete(path);
                    //}

                    await writer.WriteLineAsync(field.GetLength(0) + "\n");
                    for (int i = 0; i < field.GetLength(0); i++) {
                        if (field[i] == null) {
                            throw new NullReferenceException();
                        } else {
                            await writer.WriteLineAsync(String.Join(",", field[i]) + "\n");
                            //await File.AppendAllTextAsync(path, String.Join(",", field[i]) + "\n");
                        }
                    }
                    //playerek beirasa
                    foreach (Player player in players) {
                        await writer.WriteLineAsync(player.ToString() + "\n");
                        //await File.AppendAllTextAsync(path, player.ToString() + "\n");
                    }

                }
            } catch (Exception) {
                throw new IOException("Something went wrong saving the file");
            }
        }

    }
}
