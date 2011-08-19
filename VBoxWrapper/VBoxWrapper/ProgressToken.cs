/* 
 * Copyright 2011 Felix Rüttiger
 * 
 * This file is part of VirtualBoxService.
 *
 * VirtualBoxService is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * VirtualBoxService is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with VirtualBoxService.  If not, see <http://www.gnu.org/licenses/>.
 * 
 **/

namespace VBoxWrapper
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public abstract class ProgressToken
	{
		public virtual bool WaitTimedOut { get; protected set; }

        public abstract bool Finished { get; }

        public virtual Exception Failure { get { return null; } }


        public abstract void Cancel();
		
		public abstract void Wait();
		
        public abstract void Wait(int timeout);

        public abstract TimeSpan EstimateRemainingTime();

        protected void throwPossibleException() {
            if (Failure != null) {
                throw Failure;
            }
        }
	}
}

