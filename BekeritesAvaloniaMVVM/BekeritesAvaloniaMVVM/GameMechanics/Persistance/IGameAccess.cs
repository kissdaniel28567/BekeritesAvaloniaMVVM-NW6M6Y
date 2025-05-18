using GameMechanics.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMechanics.Persistence {
    public interface IGameAccess {
        Task<(int[][], List<Player>)> LoadAsync(String path);
        Task<(int[][], List<Player>)> LoadAsync(Stream stream);

        Task SaveAsync(String path, int[][] field, List<Player> players);
        Task SaveAsync(Stream stream, int[][] field, List<Player> players);
    }
}
