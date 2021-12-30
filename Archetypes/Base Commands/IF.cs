﻿using Meep.Tech.Data.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// An IF conditional that can run a command
      /// </summary>
      public class IF : Ows.Command.Type {

        public override IEnumerable<System.Type> ExpectedReturnTypes
          => new System.Type[] { typeof(Variable), null };

        IF()
          : base(
              new("IF"),
              new[] {
                typeof(IConditional),
                typeof(Command)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          if(context.GetUltimateParameterVariable<Boolean>(0).Value) {
            return (context.OrderedParameters[1] as Command).ExecuteUltimateCommandFor(context);
          }

          return null;
        };
      }
    }
  }
}