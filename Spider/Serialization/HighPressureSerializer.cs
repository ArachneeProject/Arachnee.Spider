//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using Spider.ArachneeCore;

//namespace Spider.Serialization
//{
//    public class HighPressureSerializer
//    {
//        private readonly string _file;
        
//        public HighPressureSerializer(string resultFilePath)
//        {
//            _file = resultFilePath;
//            File.WriteAllText(resultFilePath, string.Empty);
//        }


//        public void CompressAndWrite(string entryId, List<Connection> toCompress)
//        {
//            var builder = new StringBuilder();

//            builder.AppendLine();
//            builder.Append(CompactEntryId(entryId));
//            builder.Append(":");
//            foreach (var group in toCompress.GroupBy(c => c.Type))
//            {
//                builder.Append(((int) group.Key).ToString("X"));
//                builder.Append("_");
//                builder.Append(string.Join(",", group.Select(c => CompactEntryId(c.ConnectedId))));
//                builder.Append(";");
//            }

//            File.AppendAllText(_file, builder.ToString());
//        }

//        private string CompactEntryId(string entryId)
//        {
//            if (string.IsNullOrEmpty(entryId))
//            {
//                throw new ArgumentException("EntryId was null.");
//            }

//            var split = entryId.Split('-');

//            if (split.Length != 2)
//            {
//                throw new ArgumentException($"\"{entryId}\" is not a valid id.");
//            }

//            var entryType = split[0];
//            var tmdbId = split[1];

//            if (string.IsNullOrEmpty(entryType) || string.IsNullOrEmpty(tmdbId))
//            {
//                throw new ArgumentException($"\"{entryId}\" is not a valid id.");
//            }

//            if (!uint.TryParse(tmdbId, out var id))
//            {
//                throw new ArgumentException($"\"{entryId}\" is not a valid id.");
//            }

//            switch (entryType)
//            {
//                case nameof(Movie):
//                    return "m" + id.ToString("X");

//                case nameof(Artist):
//                    return "a" + id.ToString("X");

//                case nameof(TvSeries):
//                    return "s" + id.ToString("X");

//                default:
//                    throw new ArgumentException($"Chunk \"{entryType}\" of \"{entryId}\" is not handled.");
//            }
//        }
//    }
//}