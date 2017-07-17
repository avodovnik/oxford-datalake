using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using ImageRecognitionWrapper;
using System.Drawing;

namespace CustomImageTagger
{
    public class ImageTaggerProcessor : IProcessor
    {
        private ImageTagsGenerator tagger;
        private string imgColName;
        private string numColName;
        private string tagColName;

        public ImageTaggerProcessor(string imgCol = "ImgData", string numCol = "NumObjects", string tagCol = "Tags")
        {
            this.tagger = new ImageTagsGenerator();
            this.numColName = numCol;
            this.imgColName = imgCol;
            this.tagColName = tagCol;
        }

        public override IRow Process(IRow input, IUpdatableRow output)
        {

            var tags = this.tagger.ProduceTags(input.Get<byte[]>(this.imgColName));

            var stringOfTags = string.Join(";", tags.Keys);

            output.Set<int>(this.numColName, tags.Count);
            output.Set<string>(this.tagColName, stringOfTags);

            return output.AsReadOnly();

            //SqlMap<string, float?> sqlMap = new SqlMap<string, float?>(this.tagger.ProduceTags((byte[])input.Get<byte[]>(this.imgColName))
            //    .Select<KeyValuePair<string, float>, KeyValuePair<string, float?>>((Func<KeyValuePair<string, float>, KeyValuePair<string, float?>>)(kvp => new KeyValuePair<string, float?>(kvp.Key, new float?(kvp.Value)))));

            //output.Set<int>(this.numColName, (M0)sqlMap.get_Count());
            //output.Set<SqlMap<string, float?>>(this.tagColName, (M0)sqlMap);
            //return output.AsReadOnly();

            //throw new NotImplementedException();
        }

        private class ImageTagsGenerator
        {
            private Recognizer recognizer;

            public ImageTagsGenerator()
            {
                string currentDirectory = Directory.GetCurrentDirectory();
                this.recognizer = new Recognizer();
                this.recognizer.Initialize(currentDirectory, Environment.ProcessorCount);
                this.recognizer.AddCategoryInfo(currentDirectory + "\\category_threshold_coco.txt", "");
                this.recognizer.AddCategoryInfo(currentDirectory + "\\category_threshold_imagenet.txt", "");
            }

            public Dictionary<string, float> ProduceTags(byte[] imgData)
            {
                var results = new Dictionary<string, float>();

                this.recognizer.Compute(new Bitmap((Stream)new MemoryStream(imgData)));

                List<List<RecognizationResult>> recognizationResultListList = new List<List<RecognizationResult>>();
            
                for (int index = 0; index < this.recognizer.GetNumOfClassifiers(); ++index)
                    recognizationResultListList.Add(this.recognizer.Classify(index));

                foreach(var listOfTags in recognizationResultListList)
                {
                    foreach(var result in listOfTags)
                    {
                        if (!results.ContainsKey(result.name))
                        {
                            results.Add(result.name, result.confidence);
                        }
                    }
                }

                return results;

                //IEnumerable<RecognizationResult> source = 
                //    ((IEnumerable<List<RecognizationResult>>)recognizationResultListList).SelectMany<List<RecognizationResult>, RecognizationResult>((Func<List<RecognizationResult>, IEnumerable<RecognizationResult>>)
                //    (r => (IEnumerable<RecognizationResult>)r)).Where<RecognizationResult>((Func<RecognizationResult, bool>)(r => (bool)r.bHighlyConfident))
                //    .GroupBy<RecognizationResult, string>((Func<RecognizationResult, string>)(r => (string)r.name))
                //    .Select<IGrouping<string, RecognizationResult>, RecognizationResult>((Func<IGrouping<string, RecognizationResult>, RecognizationResult>)(r => ((IEnumerable<RecognizationResult>)r).First<RecognizationResult>()));

                //Func<RecognizationResult, string> func = (Func<RecognizationResult, string>)(r => (string)r.name);
                //Func<RecognizationResult, string> keySelector = null;
                //return source.ToDictionary<RecognizationResult, string, float>(keySelector, (Func<RecognizationResult, float>)(r => (float)r.confidence));
            }
        }
    }
}