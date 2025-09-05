namespace EpicQrizzz.Quetions.Rest.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string OptionA { get; set; } = string.Empty;
        public string OptionB { get; set; } = string.Empty;
        public string OptionC { get; set; } = string.Empty;
        public string OptionD { get; set; } = string.Empty;
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
