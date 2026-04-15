using System;
using System.IO;

namespace DummyClient
{
    public class MapData
    {
        public int MinX { get; private set; }
        public int MaxX { get; private set; }
        public int MinY { get; private set; }
        public int MaxY { get; private set; }

        bool[,] _collision;

        public void LoadMap(int mapId, string pathPrefix = "../../../../../Common/MapData")
        {
            string mapName = "Map_" + mapId.ToString("000");
            string text = File.ReadAllText($"{pathPrefix}/{mapName}.txt");
            StringReader reader = new StringReader(text);

            MinX = int.Parse(reader.ReadLine());
            MaxX = int.Parse(reader.ReadLine());
            MinY = int.Parse(reader.ReadLine());
            MaxY = int.Parse(reader.ReadLine());

            int xCount = MaxX - MinX + 1;
            int yCount = MaxY - MinY + 1;
            _collision = new bool[yCount, xCount];

            for (int y = 0; y < yCount; y++)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < xCount; x++)
                {
                    _collision[y, x] = line[x] == '1';
                }
            }
        }

        public bool CanGo(int posX, int posY)
        {
            if (posX < MinX || posX > MaxX) return false;
            if (posY < MinY || posY > MaxY) return false;

            int x = posX - MinX;
            int y = MaxY - posY;
            return !_collision[y, x];
        }
    }
}
