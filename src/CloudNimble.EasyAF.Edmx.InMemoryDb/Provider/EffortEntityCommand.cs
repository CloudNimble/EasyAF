// --------------------------------------------------------------------------------------------
// <copyright file="EffortEntityCommand.cs" company="Effort Team">
//     Copyright (C) Effort Team
//
//     Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
//     to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//     copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
//     The above copyright notice and this permission notice shall be included in
//     all copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//     OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//     THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------

namespace CloudNimble.EasyAF.Edmx.InMemoryDb.Provider
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
#if !EFOLD
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;
    using CloudNimble.EasyAF.Edmx.InMemoryDb.Internal.CommandActions;
#else
    using System.Data.Common.CommandTrees;
    using System.Data.Metadata.Edm;
#endif

    /// <summary>
    ///     Represent an Effort command that realizes Entity Framework command tree 
    ///     representations.
    /// </summary>
    public sealed class EffortEntityCommand : EffortCommandBase
    {
        private ICommandAction commandAction;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EffortEntityCommand" /> class 
        ///     based on  a provided command tree.
        /// </summary>
        /// <param name="commandtree">
        ///     The command tree that describes the operation.
        /// </param>
        public EffortEntityCommand(DbCommandTree commandtree)
        {
            commandAction = CommandActionFactory.Create(commandtree);

            foreach (KeyValuePair<string, TypeUsage> param in commandtree.Parameters)
            {
                AddParameter(param.Key);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EffortEntityCommand" /> class 
        ///     based on a prototype instance.
        /// </summary>
        /// <param name="prototype">
        ///     The prototype <see cref="EffortEntityCommand" /> object.
        /// </param>
        private EffortEntityCommand(EffortEntityCommand prototype)
        {
            commandAction = prototype.commandAction;

            foreach (EffortParameter parameter in prototype.Parameters)
            {
                AddParameter(parameter.ParameterName);
            }
        }

        /// <summary>
        ///     Executes the query.
        /// </summary>
        /// <returns>
        ///     The number of rows affected.
        /// </returns>
        public override int ExecuteNonQuery()
        {
            var context = CreateActionContext();

            return commandAction.ExecuteNonQuery(context);
        }

        /// <summary>
        ///     Executes the query and returns the first column of the first row in the result
        ///     set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <returns>
        ///     The first column of the first row in the result set.
        /// </returns>
        public override object ExecuteScalar()
        {
            var context = CreateActionContext();

            return commandAction.ExecuteScalar(context);
        }

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        public override object Clone()
        {
            return new EffortEntityCommand(this);
        }

        /// <summary>
        ///     Executes the command text against the connection.
        /// </summary>
        /// <param name="behavior">
        ///     An instance of <see cref="T:System.Data.CommandBehavior" />.
        /// </param>
        /// <returns>
        ///     A <see cref="EffortDataReader" />.
        /// </returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var context = CreateActionContext();

            return commandAction.ExecuteDataReader(context);
        }

        private ActionContext CreateActionContext()
        {
            var context = new ActionContext(EffortConnection.DbContainer);

            // Store parameters in the context
            foreach (DbParameter parameter in Parameters)
            {
                var name = parameter.ParameterName;
                var value = parameter.Value;

                if (value is not null)
                {
                    var originalType = value.GetType();

                    // Resolve enum types
                    if (originalType.IsEnum)
                    {
                        var primitive = Enum.GetUnderlyingType(originalType);
                        value = Convert.ChangeType(value, primitive);
                    }
                }

                var commandActionParameter =
                    new CommandActionParameter(name, value);

                context.Parameters.Add(commandActionParameter);
            }

            if (EffortTransaction is not null)
            {
                context.Transaction = EffortTransaction.InternalTransaction;
            }

            return context;
        }
    }
}
