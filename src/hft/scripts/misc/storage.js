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
  // Check if storage even works.
  var itWorks = false;
  if (window.localStorage) {
    try {
      window.localStorage.setItem("foobardummyburp", "blargs");
      window.localStraoge.removeItem("foobardummyburp");
      itWorks = true;
    } catch (e) {  // eslint-disable-line
    }
  }

  var noop = function() { };

  // This is an object, that way you set the name just once so calling set or get you
  // don't have to worry about getting the name wrong.
  //
  //     var fooStorage = new Storage("foo");
  //     var value = fooStorage.get();
  //     fooStorage.set(newValue);
  //     fooStorage.erase();
  var Storage = itWorks ? function(name/*, opt_path*/) {

    this.set = function(value/*, opt_days*/) {
      window.localStorage.setItem(name, value);
    };

    this.get = function() {
      return window.localStorage.getItem(name);
    };

    this.erase = function() {
      window.localStorage.removeItem(name);
    };
  } : function() {
    this.set = noop;
    this.get = noop;
    this.erase = noop;
  };

  return Storage;
});


