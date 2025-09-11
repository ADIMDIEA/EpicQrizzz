namespace EpicQrizzz.Quetions.Rest.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public char CorrectOption { get; set; } // <-- new
    }

    public class FullAwnser
    {
        public char CorrectOption { get; set; }

    }

    public class Awnser
    {
        public bool Correct { get; set; }

    }
}
