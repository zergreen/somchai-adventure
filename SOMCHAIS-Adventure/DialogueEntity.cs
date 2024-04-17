namespace SOMCHAISAdventure
{
    public class DialogueEntity
    {
        public string Name { get; set; }
        public string[] DialogueLines { get; set; }
        public int CurrentLineIndex { get; set; }

        public DialogueEntity(string name, string[] dialogueLines)
        {
            Name = name;
            DialogueLines = dialogueLines;
            CurrentLineIndex = 0;
        }

        public string GetCurrentLine()
        {
            if (CurrentLineIndex >= DialogueLines.Length)
            {
                CurrentLineIndex = 0;
            }

            return DialogueLines[CurrentLineIndex];
        }

        public void AdvanceDialogue()
        {
            CurrentLineIndex++;
        }
    }
}