package CliParser;

/**
 * Created by Mustafa on 8/29/2017.
 */
public class CliOutputParserFactory {
    static ICliOutputParser GetParser(String parsertype) {
        switch (parsertype) {
            case "CiscoShMacAddTableResult":
                return new CiscoShMacAddTableResultParser();
            default:
                throw new UnsupportedOperationException();
        }
    }
}
