$(document).ready(function(){
    $('#gallery').sliphover({
        duration: 400
    });
    $(function() {
        $.fn.typeahead.Constructor.prototype.render = function (items) {
            var that = this
            items = $(items).map(function (i, item) {
                i = $(that.options.item).attr('data-value', item)
                // Prepare the link
                var link = item.replace(/\s/g, '-').toLowerCase()
                // Replace all characters that are not A-Z, 0-9, -, _, \s
                link = link.replace(/[^A-Za-z0-9_-\s]+/g, '')
                i.find('a').attr('href', '#' + link).html(that.highlighter(item))
                return i[0]
            })
            items.first().addClass('active')
            this.$menu.html(items)
            return this
        }
    })

    $(function() {

        // make code pretty
        window.prettyPrint && prettyPrint();

        //call sliphover plugin
        $('.demo').sliphover();

    })
    var wall = new freewall("#freewall");
    wall.reset({
        selector: '.brick',
        animate: true,
        cellW: 200,
        cellH: 'auto',
        onResize: function() {
            wall.fitWidth();
        }
    });

    wall.container.find('.brick img').load(function() {
        wall.fitWidth();
    });

    //call sliphover
    $('#freewall').sliphover();
    
    $('#gallery_height').sliphover({
        height:'30%'
    });
    
    $('#gallery_reverse').sliphover({
        reverse: true
    });
});