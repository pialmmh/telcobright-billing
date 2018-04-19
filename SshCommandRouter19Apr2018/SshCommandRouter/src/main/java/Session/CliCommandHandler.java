package Session;

import com.jcraft.jsch.Channel;

import java.io.InputStream;
import java.io.PrintStream;
import java.util.ArrayList;
import java.util.List;

/**
 * Created by Mustafa on 8/8/2017.
 */
public class CliCommandHandler {
    private ISessionInfo sessionInfo;
    public int getMaxRetryForCommand() {
        return maxRetryForCommand;
    }
    public void setMaxRetryForCommand(int maxRetryForCommand) {
        this.maxRetryForCommand = maxRetryForCommand;
    }

    private int maxRetryForCommand;
    public ISessionInfo getSessionInfo() {
        return sessionInfo;
    }
    public CliCommandHandler(ISessionInfo sessionInfo,int maxRetryForCommand) {//constructor
        this.sessionInfo = sessionInfo;
        this.maxRetryForCommand = maxRetryForCommand;
    }
    public List<String> sendCommand(SingleCliCommand singleCliCommand) throws Exception {
        if (!sessionInfo.getCurrentPrompt().equals(singleCliCommand.getFromPrompt())){
            throw new Exception("Current prompt does not match from where singleCliCommand should be executed.");
        }
        List<String> output= executeCommandWithMoreChecking(singleCliCommand);
        boolean expectedPromptFound= sessionInfo.checkExpectedPrompt(output.get(output.size()-1),singleCliCommand.getExpectedPromptAfter());
        if (expectedPromptFound==false) {
            int retryCount=-1;//first time is a try, not retry
            boolean exitFlag=false;
            while (exitFlag==false)//check & set expected prompt against the sessioninfo, also retry the command
            {
                ++retryCount;
                output= executeCommandWithMoreChecking(singleCliCommand);
                expectedPromptFound= sessionInfo.checkExpectedPrompt(output.get(output.size()-1),singleCliCommand.getExpectedPromptAfter());
                if(expectedPromptFound==true||retryCount==maxRetryForCommand) exitFlag=true;
                Thread.sleep(500);
            }
        }
        return output;
    }

    private List<String> executeCommandWithMoreChecking(SingleCliCommand singleCliCommand) throws Exception {

        List<String> output = executeCommand(singleCliCommand.getCommand());
        if(!output.isEmpty())
        {
            boolean moreExists=true;
            while(moreExists=true)
            {
                String lastChunk=output.get(output.size()-1);
                if(lastChunk.contains(this.getSessionInfo().getMoreOutputIndicator())==false
                        || lastChunk.contains(singleCliCommand.getExpectedPromptAfter()))
                {
                    moreExists=false;
                }
                if(moreExists==false) break;
                //coming here means "more exists", replace the More and set the replace text as the last chunk/element of output
                output.set(output.size()-1,output.get(output.size()-1).replace(this.getSessionInfo().getMoreOutputIndicator(),""));
                List<String> moreOutput=executeCommand(this.getSessionInfo().getStrToSendForMoreOutput());
                output.addAll(moreOutput);
                Thread.sleep(10);
            }
        }
        return output;
    }
    private List<String> executeCommand(String command) throws Exception {

        Channel channel = sessionInfo.getChannel();
        InputStream readStream = channel.getInputStream();
        PrintStream writeStream = new PrintStream(sessionInfo.getChannel().getOutputStream(), true);
        writeStream.print(command + "\n");
        byte[] tmp = new byte[1024];
        List<String> allOutput = new ArrayList<String>();
        boolean exitFlag = false;
        while (true) {
            while (readStream.available() > 0) {
                int i = readStream.read(tmp, 0, 1024);
                if (i < 0) {
                    exitFlag = true;
                }
                System.out.print(new String(tmp, 0, i));
                allOutput.add(new String(tmp, 0, i));
                exitFlag = readStream.available() == 0 ? true : false;
            }
            if (exitFlag == true) break;
            if (channel.isClosed()) {
                System.out.println("exit-status: " + channel.getExitStatus());
                break;
            }
            try {
                Thread.sleep(1000);
            } catch (Exception ee) {
            }
        }
        return allOutput;
    }
}
