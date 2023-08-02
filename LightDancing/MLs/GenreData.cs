using Microsoft.ML.Data;

namespace LightDancing.MLs
{
    public class GenreData
    {
        [ColumnName("Genre"), LoadColumn(0)]
        public string Genre { get; set; }


        [ColumnName("Name"), LoadColumn(1)]
        public string Name { get; set; }


        [ColumnName("HZ_46"), LoadColumn(2)]
        public float HZ_46 { get; set; }


        [ColumnName("HZ_92"), LoadColumn(3)]
        public float HZ_92 { get; set; }


        [ColumnName("HZ_138"), LoadColumn(4)]
        public float HZ_138 { get; set; }


        [ColumnName("HZ_230"), LoadColumn(5)]
        public float HZ_230 { get; set; }


        [ColumnName("HZ_506"), LoadColumn(6)]
        public float HZ_506 { get; set; }


        [ColumnName("HZ_1012"), LoadColumn(7)]
        public float HZ_1012 { get; set; }


        [ColumnName("HZ_1978"), LoadColumn(8)]
        public float HZ_1978 { get; set; }


        [ColumnName("HZ_3956"), LoadColumn(9)]
        public float HZ_3956 { get; set; }


        [ColumnName("HZ_7912"), LoadColumn(10)]
        public float HZ_7912 { get; set; }


        [ColumnName("HZ_16054"), LoadColumn(11)]
        public float HZ_16054 { get; set; }
    }

    public class GenrePrediction
    {
        // ColumnName attribute is used to change the column name from
        // its default value, which is the name of the field.
        [ColumnName("PredictedLabel")]
        public string Prediction { get; set; }
        public float[] Score { get; set; }
    }
}
