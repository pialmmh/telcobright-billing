package CliParser;

import java.util.List;

/**
 * Created by Mustafa on 8/29/2017.
 */
public interface ICliOutputParser<T> {
    List<T> parse(String output);
}
