

function bindNavigation() {
    var $body = $('body');
    $('nav a').on('click', function (ev) {
        var lnk = $(ev.currentTarget),
            href = lnk.attr('href'),
            targetTop = $('a[name=' + href.substring(1) + ']').offset().top;

        $body.animate({ scrollTop: targetTop });
        ev.preventDefault();
    });
}

function init() {
    bindNavigation();
    demoVanilla();
    demoUpload();
}



// Full version of `log` that:
//  * Prevents errors on console methods when no console present.
//  * Exposes a global 'log' function that preserves line numbering and formatting.
(function () {
    var method;
    var noop = function () { };
    var methods = [
        'assert', 'clear', 'count', 'debug', 'dir', 'dirxml', 'error',
        'exception', 'group', 'groupCollapsed', 'groupEnd', 'info', 'log',
        'markTimeline', 'profile', 'profileEnd', 'table', 'time', 'timeEnd',
        'timeStamp', 'trace', 'warn'
    ];
    var length = methods.length;
    var console = (window.console = window.console || {});

    while (length--) {
        method = methods[length];

        // Only stub undefined methods.
        if (!console[method]) {
            console[method] = noop;
        }
    }


    if (Function.prototype.bind) {
        window.log = Function.prototype.bind.call(console.log, console);
    }
    else {
        window.log = function () {
            Function.prototype.apply.call(console.log, console, arguments);
        };
    }
})();