using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BSongFix
{
    class Program
    {
        class customData
        {
            public string[] _contributors, _customEnvironment, _customEnvironmentHash;
        }

        class BeatmapCustomData
        {
            public string _difficultyLabel;
            public double _editorOffset, _editorOldOffset;
            public string[] _warnings, _information, _suggestions, _requirements;
        }

        class NewDiffBeatmaps
        {
            public string _difficulty, _beatmapFilename;
            public double _difficultyRank, _noteJumpStartBeatOffset;
            public int _noteJumpMovementSpeed;
            BeatmapCustomData customData;
        }

        class NewDiffBeatmapSets
        {
            public string _beatmapCharacteristicName;
            public NewDiffBeatmaps[] _difficultyBeatmaps;

        }

        class OldDifLevel
        {
            public string difficulty = "", audioPath = "", jsonPath = "";
            public double offset = 0, difficultyRank = 0;
        }

        class DATInfo
        {
            public string _version, _songName, _songSubName, _songAuthorName, _levelAuthorName, _songFilename, _coverImageFilename, _environmentName;
            public double _beatsPerMinute, _songTimeOffset, _shufflePeriod, _previewStartTime, _previewDuration;
            public int _shuffle;
            public customData customData;
            public NewDiffBeatmapSets[] _difficultyBeatmapSets;

            public override string ToString()
            {

                return "file version: " + _version + "\n" + "Song name: " + _songName + "\n";
            }

        }

        class JSONInfo
        {
            public string songName, songSubName, authorName, coverImagePath, environmentName;
            public double beatsPerMinute, previewStartTime, previewDuration;
            public OldDifLevel[] difficultyLevels;
        }

        //LEVEL FILES CLASSES

        class Event
        {
            public int _type, _value;
            public double _time;
        }

        class Note
        {
            public int _lineIndex, _lineLayer, _type, _cutDirection;
            public double _time;
        }

        class Obstacle
        {
            public int _lineIndex, _type, _width;
            public double _duration, _time;
        }

        class DATLevel
        {
            public string _version;
            public string[] _BPMChanges;
            public Event[] _events;
            public Note[] _notes;
            public Obstacle[] _obstacles;
            public string[] _bookmarks;
        }

        class JSONLevel
        {
            public string _version = "1.5.0";
            public double _beatsPerMinute;
            public int _beatsPerBar = 16;
            public int _noteJumpSpeed = 10;
            public int _shuffle = 0;
            public double _shufflePeriod = 0.5;
            public Event[] _events;
            public Note[] _notes;
            public Obstacle[] _obstacles;
            public string[] _bookmarks;
        }
        

        static void Main(string[] args)
        {

            //Console.WriteLine(Path.GetFileNameWithoutExtension("\\Expert.dat"));
            JSONInfo fileInfo = new JSONInfo();

            string info = "\\info.dat";

            string Json2Read = "";
            using (StreamReader sr = new StreamReader(args[0] + info))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    Json2Read += line;
                }
            }

            DATInfo deserializedDATInfo = JsonConvert.DeserializeObject<DATInfo>(Json2Read);
            NewDiffBeatmaps[] levels = deserializedDATInfo._difficultyBeatmapSets[0]._difficultyBeatmaps;

            fileInfo.songName = deserializedDATInfo._songName;
            fileInfo.songSubName = deserializedDATInfo._songAuthorName;
            fileInfo.authorName = deserializedDATInfo._levelAuthorName;
            fileInfo.beatsPerMinute = deserializedDATInfo._beatsPerMinute;
            fileInfo.previewStartTime = deserializedDATInfo._previewStartTime;
            fileInfo.previewDuration = deserializedDATInfo._previewDuration;
            fileInfo.coverImagePath = deserializedDATInfo._coverImageFilename;
            fileInfo.environmentName = deserializedDATInfo._environmentName;
            fileInfo.difficultyLevels = new OldDifLevel[levels.Length];

            for (int i = 0; i < levels.Length; i++)
            {
                //Console.WriteLine("Current index: " + i);
                string songPath = deserializedDATInfo._songFilename;
                string jsonPath = levels[i]._beatmapFilename;
                jsonPath = Path.ChangeExtension(jsonPath, ".json");
                songPath = Path.ChangeExtension(songPath, ".ogg");

                if (fileInfo.difficultyLevels[i] == null)
                {
                    fileInfo.difficultyLevels[i] = new OldDifLevel();
                }

                fileInfo.difficultyLevels[i].difficulty = levels[i]._difficulty;
                fileInfo.difficultyLevels[i].difficultyRank = levels[i]._difficultyRank;
                fileInfo.difficultyLevels[i].audioPath = songPath;
                fileInfo.difficultyLevels[i].jsonPath = jsonPath;
                fileInfo.difficultyLevels[i].offset = levels[i]._noteJumpStartBeatOffset;
            }

            string json = JsonConvert.SerializeObject(fileInfo);
            string savePath = Path.ChangeExtension(args[0] + info, ".json");

            using (StreamWriter sw = new StreamWriter(savePath))
            {
                sw.Write(json);
            }

            int j = levels.Length - 1;
            while (j >= 0)
            {
                //Console.WriteLine("While index: " + j);
                string levelPath = args[0] + "\\" + levels[j]._beatmapFilename;
                if (File.Exists(levelPath))
                {
                    string difName = Path.GetFileNameWithoutExtension(levelPath);

                    Console.WriteLine(difName);

                    JSONLevel jsonFile = new JSONLevel();

                    string datJson = "";
                    using (StreamReader sr = new StreamReader(levelPath))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            datJson += line;
                        }
                    }

                    DATLevel deserializedDATLevel = JsonConvert.DeserializeObject<DATLevel>(datJson);

                    jsonFile._beatsPerMinute = fileInfo.beatsPerMinute;
                    jsonFile._shuffle = deserializedDATInfo._shuffle;
                    jsonFile._shufflePeriod = deserializedDATInfo._shufflePeriod;
                    jsonFile._events = deserializedDATLevel._events;
                    jsonFile._notes = deserializedDATLevel._notes;
                    jsonFile._obstacles = deserializedDATLevel._obstacles;
                    jsonFile._bookmarks = deserializedDATLevel._bookmarks;
                    for (int i = 0; i < levels.Length; i++)
                    {
                        if (levels[i]._difficulty == difName)
                        {
                            jsonFile._noteJumpSpeed = levels[i]._noteJumpMovementSpeed;
                        }
                    }

                    string jsonLevel2Write = JsonConvert.SerializeObject(jsonFile);
                    levelPath = Path.ChangeExtension(levelPath, ".json");

                    using (StreamWriter sw = new StreamWriter(levelPath))
                    {
                        sw.Write(jsonLevel2Write);
                    }


                }

                j--;
            }

            string audioFilePath = args[0] + "\\" + deserializedDATInfo._songFilename;
            File.Move(audioFilePath, Path.ChangeExtension(audioFilePath, ".ogg"));



        }
    }
}
