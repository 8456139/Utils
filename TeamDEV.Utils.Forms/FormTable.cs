/**
 *  MIT License
 *
 *  Copyright (c) 2019 SlaneR
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 **/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace TeamDEV.Utils.Forms {
    /// <summary>
    /// Provides <see cref="Form"/> registration for accessing specific <see cref="Form"/> globally.
    /// </summary>
    public static class FormTable {
        private static readonly Dictionary<Type, Form> mRegisteredForms =
            new Dictionary<Type, Form>();
        
        /// <summary>
        /// Register a form
        /// </summary>
        /// <param name="lazyInitialization">If true, <see cref="Form"/> will created when accessing it</param>
        /// <typeparam name="T">Type of form</typeparam>
        /// <returns>Registration result</returns>
        public static bool Register<T>(bool lazyInitialization = true) where T : Form {
            Type t = typeof(T);

            // If already registered, return true
            if (mRegisteredForms.ContainsKey(t)) return true;

            try {
                // If lazyInitialization is true,
                // we'll create it's instance when accessing to it.
                T value = lazyInitialization ? default(T) : CreateFormInstance<T>();
                mRegisteredForms.Add(t, value);
                return true;
            }
            catch {
                return false;
            }
        }
        /// <summary>
        /// Unregister a form
        /// </summary>
        /// <param name="dispose">Dispose after unregister if true</param>
        /// <typeparam name="T">Type of form</typeparam>
        /// <returns>Unregistration result</returns>
        public static bool Unregister<T>(bool dispose = false) where T : Form {
            Type t = typeof(T);

            // If the type is not registered, return false
            if (!mRegisteredForms.ContainsKey(t)) return false;

            try {
                // We can't predict form is currently in use or not
                // so, user should call Unregister when finish it's uses
                Form form = mRegisteredForms[t];

                // If form is not null, we can check dispose
                if (form != null && dispose) form.Dispose();

                // Return result of remove
                return mRegisteredForms.Remove(t);
            } catch {
                return false;
            }
        }
        /// <summary>
        /// Unregister all forms, but not disposing
        /// </summary>
        public static void UnregisterAll() {
            mRegisteredForms.Clear();
        }
        /// <summary>
        /// Unregister and dispose all forms
        /// </summary>
        public static void Cleanup() {
            foreach (Form form in mRegisteredForms.Values) {
                if (!form.IsDisposed) form.Dispose();
            }

            mRegisteredForms.Clear();
        }
        /// <summary>
        /// Check specified type is registered or not.
        /// </summary>
        /// <typeparam name="T">Form type to check</typeparam>
        /// <returns>True if registered, false otherwise</returns>
        public static bool IsRegistered<T>() where T : Form {
            return mRegisteredForms.ContainsKey(typeof(T));
        }
        /// <summary>
        /// Get an instance of registered <see cref="Form"/>
        /// </summary>
        /// <typeparam name="T">A type of form</typeparam>
        /// <param name="reInitializeWhenDisposed">If this value is true and form is disposed, create instance again</param>
        /// <returns>An instance of <see cref="Form"/></returns>
        public static T Get<T>(bool reInitializeWhenDisposed = true) where T : Form {
            Type t = typeof(T);

            // If T is not registered, return null
            if (!mRegisteredForms.ContainsKey(t)) return default(T);

            // If form registered as lazy initialization
            // we'll create it's instance here
            if (mRegisteredForms[t] == null) {
                try {
                    // Currently only default constructor is supported.
                    // If we can't create instance by calling default constructor
                    // it'll throw an exception.
                    mRegisteredForms[t] = CreateFormInstance<T>();
                } catch {
                    return default(T);
                }
            } else {
                // If form is disposed and reIntializeWhenDisposed is true
                // we'll create form's instance again.
                if (mRegisteredForms[t].IsDisposed && reInitializeWhenDisposed) {
                    mRegisteredForms[t] = CreateFormInstance<T>();
                }
            }

            return mRegisteredForms[t] as T;
        }
        
        private static T CreateFormInstance<T>() where T : Form {
            // Create NewExpression for given type.
            NewExpression newExpr = Expression.New(typeof(T));
            LambdaExpression lambda = Expression.Lambda(newExpr);
            Delegate ctor = lambda.Compile();

            // Invoke constructor and return it's result
            return ctor.DynamicInvoke() as T;
        }
    }
}