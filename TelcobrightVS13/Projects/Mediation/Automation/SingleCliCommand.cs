﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelcobrightMediation.Automation
{
    public class SingleCliCommand
    {
        private String command;
        private String fromPrompt;
        private String expectedPromptAfter;

        public SingleCliCommand(String command, String fromPrompt, String expectedPromptAfter)
        {
            this.command = command;
            this.fromPrompt = fromPrompt;
            this.expectedPromptAfter = expectedPromptAfter;
        }

        public String getCommand()
        {
            return command;
        }

        public void setCommand(String command)
        {
            this.command = command;
        }

        public String getFromPrompt()
        {
            return fromPrompt;
        }

        public void setFromPrompt(String fromPrompt)
        {
            this.fromPrompt = fromPrompt;
        }

        public String getExpectedPromptAfter()
        {
            return expectedPromptAfter;
        }

        public void setExpectedPromptAfter(String expectedPromptAfter)
        {
            this.expectedPromptAfter = expectedPromptAfter;
        }
    }

}
