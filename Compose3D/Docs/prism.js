/* http://prismjs.com/download.html?themes=prism&languages=clike+csharp&plugins=toolbar+highlight-keywords */
var _self = (typeof window !== 'undefined')
	? window   // if in browser
	: (
		(typeof WorkerGlobalScope !== 'undefined' && self instanceof WorkerGlobalScope)
		? self // if in worker
		: {}   // if in node js
	);

/**
 * Prism: Lightweight, robust, elegant syntax highlighting
 * MIT license http://www.opensource.org/licenses/mit-license.php/
 * @author Lea Verou http://lea.verou.me
 */

var Prism = (function () {

    // Private helper vars
    var lang = /\blang(?:uage)?-(\w+)\b/i;
    var uniqueId = 0;

    var _ = _self.Prism = {
        manual: _self.Prism && _self.Prism.manual,
        util: {
            encode: function (tokens) {
                if (tokens instanceof Token) {
                    return new Token(tokens.type, _.util.encode(tokens.content), tokens.alias);
                } else if (_.util.type(tokens) === 'Array') {
                    return tokens.map(_.util.encode);
                } else {
                    return tokens.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/\u00a0/g, ' ');
                }
            },

            type: function (o) {
                return Object.prototype.toString.call(o).match(/\[object (\w+)\]/)[1];
            },

            objId: function (obj) {
                if (!obj['__id']) {
                    Object.defineProperty(obj, '__id', { value: ++uniqueId });
                }
                return obj['__id'];
            },

            // Deep clone a language definition (e.g. to extend it)
            clone: function (o) {
                var type = _.util.type(o);

                switch (type) {
                    case 'Object':
                        var clone = {};

                        for (var key in o) {
                            if (o.hasOwnProperty(key)) {
                                clone[key] = _.util.clone(o[key]);
                            }
                        }

                        return clone;

                    case 'Array':
                        // Check for existence for IE8
                        return o.map && o.map(function (v) { return _.util.clone(v); });
                }

                return o;
            }
        },

        languages: {
            extend: function (id, redef) {
                var lang = _.util.clone(_.languages[id]);

                for (var key in redef) {
                    lang[key] = redef[key];
                }

                return lang;
            },

            /**
             * Insert a token before another token in a language literal
             * As this needs to recreate the object (we cannot actually insert before keys in object literals),
             * we cannot just provide an object, we need anobject and a key.
             * @param inside The key (or language id) of the parent
             * @param before The key to insert before. If not provided, the function appends instead.
             * @param insert Object with the key/value pairs to insert
             * @param root The object that contains `inside`. If equal to Prism.languages, it can be omitted.
             */
            insertBefore: function (inside, before, insert, root) {
                root = root || _.languages;
                var grammar = root[inside];

                if (arguments.length == 2) {
                    insert = arguments[1];

                    for (var newToken in insert) {
                        if (insert.hasOwnProperty(newToken)) {
                            grammar[newToken] = insert[newToken];
                        }
                    }

                    return grammar;
                }

                var ret = {};

                for (var token in grammar) {

                    if (grammar.hasOwnProperty(token)) {

                        if (token == before) {

                            for (var newToken in insert) {

                                if (insert.hasOwnProperty(newToken)) {
                                    ret[newToken] = insert[newToken];
                                }
                            }
                        }

                        ret[token] = grammar[token];
                    }
                }

                // Update references in other language definitions
                _.languages.DFS(_.languages, function (key, value) {
                    if (value === root[inside] && key != inside) {
                        this[key] = ret;
                    }
                });

                return root[inside] = ret;
            },

            // Traverse a language definition with Depth First Search
            DFS: function (o, callback, type, visited) {
                visited = visited || {};
                for (var i in o) {
                    if (o.hasOwnProperty(i)) {
                        callback.call(o, i, o[i], type || i);

                        if (_.util.type(o[i]) === 'Object' && !visited[_.util.objId(o[i])]) {
                            visited[_.util.objId(o[i])] = true;
                            _.languages.DFS(o[i], callback, null, visited);
                        }
                        else if (_.util.type(o[i]) === 'Array' && !visited[_.util.objId(o[i])]) {
                            visited[_.util.objId(o[i])] = true;
                            _.languages.DFS(o[i], callback, i, visited);
                        }
                    }
                }
            }
        },
        plugins: {},

        highlightAll: function (async, callback) {
            var env = {
                callback: callback,
                selector: 'code[class*="language-"], [class*="language-"] code, code[class*="lang-"], [class*="lang-"] code'
            };

            _.hooks.run("before-highlightall", env);

            var elements = env.elements || document.querySelectorAll(env.selector);

            for (var i = 0, element; element = elements[i++];) {
                _.highlightElement(element, async === true, env.callback);
            }
        },

        highlightElement: function (element, async, callback) {
            // Find language
            var language, grammar, parent = element;

            while (parent && !lang.test(parent.className)) {
                parent = parent.parentNode;
            }

            if (parent) {
                language = (parent.className.match(lang) || [, ''])[1].toLowerCase();
                grammar = _.languages[language];
            }

            // Set language on the element, if not present
            element.className = element.className.replace(lang, '').replace(/\s+/g, ' ') + ' language-' + language;

            // Set language on the parent, for styling
            parent = element.parentNode;

            if (/pre/i.test(parent.nodeName)) {
                parent.className = parent.className.replace(lang, '').replace(/\s+/g, ' ') + ' language-' + language;
            }

            var code = element.textContent;

            var env = {
                element: element,
                language: language,
                grammar: grammar,
                code: code
            };

            _.hooks.run('before-sanity-check', env);

            if (!env.code || !env.grammar) {
                if (env.code) {
                    env.element.textContent = env.code;
                }
                _.hooks.run('complete', env);
                return;
            }

            _.hooks.run('before-highlight', env);

            if (async && _self.Worker) {
                var worker = new Worker(_.filename);

                worker.onmessage = function (evt) {
                    env.highlightedCode = evt.data;

                    _.hooks.run('before-insert', env);

                    env.element.innerHTML = env.highlightedCode;

                    callback && callback.call(env.element);
                    _.hooks.run('after-highlight', env);
                    _.hooks.run('complete', env);
                };

                worker.postMessage(JSON.stringify({
                    language: env.language,
                    code: env.code,
                    immediateClose: true
                }));
            }
            else {
                env.highlightedCode = _.highlight(env.code, env.grammar, env.language);

                _.hooks.run('before-insert', env);

                env.element.innerHTML = env.highlightedCode;

                callback && callback.call(element);

                _.hooks.run('after-highlight', env);
                _.hooks.run('complete', env);
            }
        },

        highlight: function (text, grammar, language) {
            var tokens = _.tokenize(text, grammar);
            return Token.stringify(_.util.encode(tokens), language);
        },

        tokenize: function (text, grammar, language) {
            var Token = _.Token;

            var strarr = [text];

            var rest = grammar.rest;

            if (rest) {
                for (var token in rest) {
                    grammar[token] = rest[token];
                }

                delete grammar.rest;
            }

            tokenloop: for (var token in grammar) {
                if (!grammar.hasOwnProperty(token) || !grammar[token]) {
                    continue;
                }

                var patterns = grammar[token];
                patterns = (_.util.type(patterns) === "Array") ? patterns : [patterns];

                for (var j = 0; j < patterns.length; ++j) {
                    var pattern = patterns[j],
                        inside = pattern.inside,
                        lookbehind = !!pattern.lookbehind,
                        greedy = !!pattern.greedy,
                        lookbehindLength = 0,
                        alias = pattern.alias;

                    if (greedy && !pattern.pattern.global) {
                        // Without the global flag, lastIndex won't work
                        var flags = pattern.pattern.toString().match(/[imuy]*$/)[0];
                        pattern.pattern = RegExp(pattern.pattern.source, flags + "g");
                    }

                    pattern = pattern.pattern || pattern;

                    // Don’t cache length as it changes during the loop
                    for (var i = 0, pos = 0; i < strarr.length; pos += strarr[i].length, ++i) {

                        var str = strarr[i];

                        if (strarr.length > text.length) {
                            // Something went terribly wrong, ABORT, ABORT!
                            break tokenloop;
                        }

                        if (str instanceof Token) {
                            continue;
                        }

                        pattern.lastIndex = 0;

                        var match = pattern.exec(str),
                            delNum = 1;

                        // Greedy patterns can override/remove up to two previously matched tokens
                        if (!match && greedy && i != strarr.length - 1) {
                            pattern.lastIndex = pos;
                            match = pattern.exec(text);
                            if (!match) {
                                break;
                            }

                            var from = match.index + (lookbehind ? match[1].length : 0),
                                to = match.index + match[0].length,
                                k = i,
                                p = pos;

                            for (var len = strarr.length; k < len && p < to; ++k) {
                                p += strarr[k].length;
                                // Move the index i to the element in strarr that is closest to from
                                if (from >= p) {
                                    ++i;
                                    pos = p;
                                }
                            }

                            /*
                             * If strarr[i] is a Token, then the match starts inside another Token, which is invalid
                             * If strarr[k - 1] is greedy we are in conflict with another greedy pattern
                             */
                            if (strarr[i] instanceof Token || strarr[k - 1].greedy) {
                                continue;
                            }

                            // Number of tokens to delete and replace with the new match
                            delNum = k - i;
                            str = text.slice(pos, p);
                            match.index -= pos;
                        }

                        if (!match) {
                            continue;
                        }

                        if (lookbehind) {
                            lookbehindLength = match[1].length;
                        }

                        var from = match.index + lookbehindLength,
                            match = match[0].slice(lookbehindLength),
                            to = from + match.length,
                            before = str.slice(0, from),
                            after = str.slice(to);

                        var args = [i, delNum];

                        if (before) {
                            args.push(before);
                        }

                        var wrapped = new Token(token, inside ? _.tokenize(match, inside) : match, alias, match, greedy);

                        args.push(wrapped);

                        if (after) {
                            args.push(after);
                        }

                        Array.prototype.splice.apply(strarr, args);
                    }
                }
            }

            return strarr;
        },

        hooks: {
            all: {},

            add: function (name, callback) {
                var hooks = _.hooks.all;

                hooks[name] = hooks[name] || [];

                hooks[name].push(callback);
            },

            run: function (name, env) {
                var callbacks = _.hooks.all[name];

                if (!callbacks || !callbacks.length) {
                    return;
                }

                for (var i = 0, callback; callback = callbacks[i++];) {
                    callback(env);
                }
            }
        }
    };

    var Token = _.Token = function (type, content, alias, matchedStr, greedy) {
        this.type = type;
        this.content = content;
        this.alias = alias;
        // Copy of the full string this token was created from
        this.length = (matchedStr || "").length | 0;
        this.greedy = !!greedy;
    };

    Token.stringify = function (o, language, parent) {
        if (typeof o == 'string') {
            return o;
        }

        if (_.util.type(o) === 'Array') {
            return o.map(function (element) {
                return Token.stringify(element, language, o);
            }).join('');
        }

        var env = {
            type: o.type,
            content: Token.stringify(o.content, language, parent),
            tag: 'span',
            classes: ['token', o.type],
            attributes: {},
            language: language,
            parent: parent
        };

        if (env.type == 'comment') {
            env.attributes['spellcheck'] = 'true';
        }

        if (o.alias) {
            var aliases = _.util.type(o.alias) === 'Array' ? o.alias : [o.alias];
            Array.prototype.push.apply(env.classes, aliases);
        }

        _.hooks.run('wrap', env);

        var attributes = Object.keys(env.attributes).map(function (name) {
            return name + '="' + (env.attributes[name] || '').replace(/"/g, '&quot;') + '"';
        }).join(' ');

        return '<' + env.tag + ' class="' + env.classes.join(' ') + '"' + (attributes ? ' ' + attributes : '') + '>' + env.content + '</' + env.tag + '>';

    };

    if (!_self.document) {
        if (!_self.addEventListener) {
            // in Node.js
            return _self.Prism;
        }
        // In worker
        _self.addEventListener('message', function (evt) {
            var message = JSON.parse(evt.data),
                lang = message.language,
                code = message.code,
                immediateClose = message.immediateClose;

            _self.postMessage(_.highlight(code, _.languages[lang], lang));
            if (immediateClose) {
                _self.close();
            }
        }, false);

        return _self.Prism;
    }

    //Get current script and highlight
    var script = document.currentScript || [].slice.call(document.getElementsByTagName("script")).pop();

    if (script) {
        _.filename = script.src;

        if (document.addEventListener && !_.manual && !script.hasAttribute('data-manual')) {
            if (document.readyState !== "loading") {
                if (window.requestAnimationFrame) {
                    window.requestAnimationFrame(_.highlightAll);
                } else {
                    window.setTimeout(_.highlightAll, 16);
                }
            }
            else {
                document.addEventListener('DOMContentLoaded', _.highlightAll);
            }
        }
    }

    return _self.Prism;

})();

if (typeof module !== 'undefined' && module.exports) {
    module.exports = Prism;
}

// hack for components to work correctly in node.js
if (typeof global !== 'undefined') {
    global.Prism = Prism;
}
;
Prism.languages.clike = {
    'comment': [
		{
		    pattern: /(^|[^\\])\/\*[\w\W]*?\*\//,
		    lookbehind: true
		},
		{
		    pattern: /(^|[^\\:])\/\/.*/,
		    lookbehind: true
		}
    ],
    'string': {
        pattern: /(["'])(\\(?:\r\n|[\s\S])|(?!\1)[^\\\r\n])*\1/,
        greedy: true
    },
    'class-name': {
        pattern: /((?:\b(?:class|interface|extends|implements|trait|instanceof|new)\s+)|(?:catch\s+\())[a-z0-9_\.\\]+/i,
        lookbehind: true,
        inside: {
            punctuation: /(\.|\\)/
        }
    },
    'keyword': /\b(if|else|while|do|for|return|in|instanceof|function|new|try|throw|catch|finally|null|break|continue)\b/,
    'boolean': /\b(true|false)\b/,
    'function': /[a-z0-9_]+(?=\()/i,
    'number': /\b-?(?:0x[\da-f]+|\d*\.?\d+(?:e[+-]?\d+)?)\b/i,
    'operator': /--?|\+\+?|!=?=?|<=?|>=?|==?=?|&&?|\|\|?|\?|\*|\/|~|\^|%/,
    'punctuation': /[{}[\];(),.:]/
};

Prism.languages.csharp = Prism.languages.extend('clike', {
    'keyword': /\b(abstract|as|async|await|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while|add|alias|ascending|async|await|descending|dynamic|from|get|global|group|into|join|let|orderby|partial|remove|select|set|value|var|where|yield)\b/,
    'string': [
		{
		    pattern: /@("|')(\1\1|\\\1|\\?(?!\1)[\s\S])*\1/,
		    greedy: true
		},
		{
		    pattern: /("|')(\\?.)*?\1/,
		    greedy: true
		}
    ],
    'number': /\b-?(0x[\da-f]+|\d*\.?\d+f?)\b/i
});

Prism.languages.insertBefore('csharp', 'keyword', {
    'generic-method': {
        pattern: /[a-z0-9_]+\s*<[^>\r\n]+?>\s*(?=\()/i,
        alias: 'function',
        inside: {
            keyword: Prism.languages.csharp.keyword,
            punctuation: /[<>(),.:]/
        }
    },
    'preprocessor': {
        pattern: /(^\s*)#.*/m,
        lookbehind: true,
        alias: 'property',
        inside: {
            // highlight preprocessor directives as keywords
            'directive': {
                pattern: /(\s*#)\b(define|elif|else|endif|endregion|error|if|line|pragma|region|undef|warning)\b/,
                lookbehind: true,
                alias: 'keyword'
            }
        }
    }
});

(function () {
    if (typeof self === 'undefined' || !self.Prism || !self.document) {
        return;
    }

    var callbacks = [];
    var map = {};
    var noop = function () { };

    Prism.plugins.toolbar = {};

    /**
	 * Register a button callback with the toolbar.
	 *
	 * @param {string} key
	 * @param {Object|Function} opts
	 */
    var registerButton = Prism.plugins.toolbar.registerButton = function (key, opts) {
        var callback;

        if (typeof opts === 'function') {
            callback = opts;
        } else {
            callback = function (env) {
                var element;

                if (typeof opts.onClick === 'function') {
                    element = document.createElement('button');
                    element.type = 'button';
                    element.addEventListener('click', function () {
                        opts.onClick.call(this, env);
                    });
                } else if (typeof opts.url === 'string') {
                    element = document.createElement('a');
                    element.href = opts.url;
                } else {
                    element = document.createElement('span');
                }

                element.textContent = opts.text;

                return element;
            };
        }

        callbacks.push(map[key] = callback);
    };

    /**
	 * Post-highlight Prism hook callback.
	 *
	 * @param env
	 */
    var hook = Prism.plugins.toolbar.hook = function (env) {
        // Check if inline or actual code block (credit to line-numbers plugin)
        var pre = env.element.parentNode;
        if (!pre || !/pre/i.test(pre.nodeName)) {
            return;
        }

        // Autoloader rehighlights, so only do this once.
        if (pre.classList.contains('code-toolbar')) {
            return;
        }

        pre.classList.add('code-toolbar');

        // Setup the toolbar
        var toolbar = document.createElement('div');
        toolbar.classList.add('toolbar');

        if (document.body.hasAttribute('data-toolbar-order')) {
            callbacks = document.body.getAttribute('data-toolbar-order').split(',').map(function (key) {
                return map[key] || noop;
            });
        }

        callbacks.forEach(function (callback) {
            var element = callback(env);

            if (!element) {
                return;
            }

            var item = document.createElement('div');
            item.classList.add('toolbar-item');

            item.appendChild(element);
            toolbar.appendChild(item);
        });

        // Add our toolbar to the <pre> tag
        pre.appendChild(toolbar);
    };

    registerButton('label', function (env) {
        var pre = env.element.parentNode;
        if (!pre || !/pre/i.test(pre.nodeName)) {
            return;
        }

        if (!pre.hasAttribute('data-label')) {
            return;
        }

        var element, template;
        var text = pre.getAttribute('data-label');
        try {
            // Any normal text will blow up this selector.
            template = document.querySelector('template#' + text);
        } catch (e) { }

        if (template) {
            element = template.content;
        } else {
            if (pre.hasAttribute('data-url')) {
                element = document.createElement('a');
                element.href = pre.getAttribute('data-url');
            } else {
                element = document.createElement('span');
            }

            element.textContent = text;
        }

        return element;
    });

    /**
	 * Register the toolbar with Prism.
	 */
    Prism.hooks.add('complete', hook);
})();

(function () {

    if (
        typeof self !== 'undefined' && !self.Prism ||
        typeof global !== 'undefined' && !global.Prism
    ) {
        return;
    }

    Prism.hooks.add('wrap', function (env) {
        if (env.type !== "keyword") {
            return;
        }
        env.classes.push('keyword-' + env.content);
    });

})();

