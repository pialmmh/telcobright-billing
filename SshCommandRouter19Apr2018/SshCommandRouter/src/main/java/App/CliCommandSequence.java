package App;

import Session.SingleCliCommand;

import java.util.*;

/**
 * Created by Mustafa on 8/8/2017.
 */
public class CliCommandSequence {
    private List<SingleCliCommand> commands;
    private String strToSendForMoreOutput;
    private long sleepIntervalBetweenCommands;

    public void init() {
        commands = new ArrayList<SingleCliCommand>();
        commands.add(new SingleCliCommand("en 5", "DHK-7609-01>", "Password: "));
        commands.add(new SingleCliCommand("M@ng0N0C", "Password: ", "DHK-7609-01#"));
        commands.add(new SingleCliCommand("show mac address-table", "DHK-7609-01#",
              "DHK-7609-01#"));


        //zte sbc test
        /*commands.add(new SingleCliCommand("en", "SBC-new-up>", "Password:"));
        commands.add(new SingleCliCommand("zxr10", "Password:", "SBC-new-up#"));
        commands.add(new SingleCliCommand("conf t", "SBC-new-up#", "SBC-new-up(config)#"));
        commands.add(new SingleCliCommand("sbc", "SBC-new-up(config)#", "SBC-new-up(config-sbc)#"));
        commands.add(new SingleCliCommand("system", "SBC-new-up(config-sbc)#", "SBC-new-up(config-sbc-system)#"));
        commands.add(new SingleCliCommand("no activate signal-group 3511", "SBC-new-up(config-sbc-system)#", "SBC-new-up(config-sbc-system)#"));
        commands.add(new SingleCliCommand("end", "SBC-new-up(config-sbc-system)#", "SBC-new-up#"));
        commands.add(new SingleCliCommand("wr", "SBC-new-up#","SBC-new-up#"));
        commands.add(new SingleCliCommand("show run", "SBC-new-up#","SBC-new-up#"));*/
    }

    public long getSleepIntervalBetweenCommands() {
        return sleepIntervalBetweenCommands;
    }

    public void setSleepIntervalBetweenCommands(long sleepIntervalBetweenCommands) {
        this.sleepIntervalBetweenCommands = sleepIntervalBetweenCommands;
    }

    public void setCommands(List<SingleCliCommand> commands) {
        this.commands = commands;
    }

    public List<SingleCliCommand> getCommands() {
        return commands;
    }

    public String getStrToSendForMoreOutput() {
        return strToSendForMoreOutput;
    }

    public void setStrToSendForMoreOutput(String strToSendForMoreOutput) {
        this.strToSendForMoreOutput = strToSendForMoreOutput;
    }
}
