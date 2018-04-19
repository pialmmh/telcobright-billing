import App.CliCommandSequence;
import Session.SshSessionInfo;

/**
 * Created by Gigabyte on 4/19/2018.
 */
public class SshCommandSender {

    private CliCommandSequence cliCommandSequence;
    private SshSessionInfo sessionInfo;

    public CliCommandSequence getCliCommandSequence() {
        return cliCommandSequence;
    }

    public void setCliCommandSequence(CliCommandSequence cliCommandSequence) {
        this.cliCommandSequence = cliCommandSequence;
    }

    public SshSessionInfo getSessionInfo() {
        return sessionInfo;
    }

    public void setSessionInfo(SshSessionInfo sessionInfo) {
        this.sessionInfo = sessionInfo;
    }

    public String sendCommand() throws Exception
    {
        return this.sessionInfo.getOutputFromSession(cliCommandSequence);
    }
}
