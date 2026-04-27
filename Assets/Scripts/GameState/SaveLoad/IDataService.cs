using System.Collections.Generic;

namespace GameState.SaveLoad
{
    public interface IDataService
    {
        void Save(GameDataDTO data, bool overwrite = true);
        GameDataDTO Load(string name);
        void Delete(string name);
        void DeleteAll();
        IEnumerable<string> ListSaves();
    }
}