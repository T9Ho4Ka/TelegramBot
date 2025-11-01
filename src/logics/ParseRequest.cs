namespace TelegramBot.logics;

public class Parser {
    
    public static void ParseRequest(ref Message msg,
        out string command,
        out string mention,
        out string args,
        out List<string> flags, out bool isReply) {
        
        mention = string.Empty;
        args = string.Empty;
        flags = new();
        isReply = msg.ReplyToMessage != null;
        var request = msg.Text?.Trim()[1..] ?? ""; //trimmed request 
        //==============
        //   Command
        //==============
        var endOfCommand = request.IndexOf(' ');
        if (endOfCommand < 0) {
            endOfCommand = request.Length;
            command = request[..endOfCommand];
            return;
        }
         //if only the command was sent
        command = request[..endOfCommand];
        request = request[(command.Length + 1)..].Trim(); //-command
    
    
        //==============
        //   Mention
        //==============
        var mentionEntity = msg.Entities?.FirstOrDefault(e => e.Type == MessageEntityType.Mention);
        if (mentionEntity != null) {
            mention = msg.Text.Substring(
                startIndex: mentionEntity.Offset,
                length: mentionEntity.Length);
            request = request.Replace(mention, "").Trim();
        }
        //else -> me
        if (string.IsNullOrEmpty(request)) return; // command mention.
        
        //==============
        //   flags
        //==============
        while (true){
            var flagIndex = request.IndexOf(Constants.OptionPrefix);
            if (flagIndex < 0) break; //если флагов нет.
        
            var flagEnd = request.IndexOf(' ', flagIndex);
            if (flagEnd < 0) flagEnd = request.Length; // and break
            
            var flag = request.Substring(flagIndex, flagEnd - flagIndex);
            if (flag.Length == 2) flags.Add(flag);
            request = request.Remove(flagIndex, flag.Length).Trim();
        }
        if (string.IsNullOrEmpty(request)) return; //command flag/command mention flag
        
        //==============
        //   args
        //==============
        
        var isArgs = request.IndexOf(' '); // the end args
        if (isArgs > 0) args = request[..(isArgs+1)];
        else args = request;
    }
}