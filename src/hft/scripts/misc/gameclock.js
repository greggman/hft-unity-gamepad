/*
 * Copyright 2014, Gregg Tavares.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 *     * Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above
 * copyright notice, this list of conditions and the following disclaimer
 * in the documentation and/or other materials provided with the
 * distribution.
 *     * Neither the name of Gregg Tavares. nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

"use strict";

define(function() {

  var LocalClock = function() {
    this.getTime = function() {
      return Date.now() * 0.001;
    };
  };

  /**
   * A clock that returns the time elapsed since the last time it
   * was queried
   * @constructor
   * @alias GameClock
   * @param {Clock?} The clock to use for this GameClock (pass it
   *        a SyncedClock of you want the clock to be synced or
   *        nothing if you just want a local clock.
   */
  var GameClock = function(clock) {
    clock = clock || new LocalClock();

    this.gameTime = 0;
    this.callCount = 0;

    var then = clock.getTime();

    /**
     * Gets the time elapsed in seconds since the last time this was
     * called
     * @return {number} The time elapsed in seconds since this was
     *     last called. Note will never return a time more than
     *     1/20th of second. This is to help prevent giant time
     *     range that might overflow the math on a game.
     *
     */
    this.getElapsedTime = function() {
      ++this.callCount;

      var now = clock.getTime();
      var elapsedTime = Math.min(now - then, 0.05);  // Don't return a time less than 0.05 second
      this.gameTime += elapsedTime;

      then = now;

      return elapsedTime;
    }.bind(this);
  };

  return GameClock;
});
