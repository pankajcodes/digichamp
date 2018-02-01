////////////////////////////////////////////////////////////////////////////// chart js
	var chart = new Chartist.Line('#main_chart', {
		labels: ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10', '11', '12'],
		series: [
		[12, 11, 10, 9, 8, 10, 8, 10, 8, 12, 10, 12,14],
		[2, 5,7, 4, 6, 4, 6, 7, 6, 8, 6, 8, 6 ]
		]
	}, {
		low: 0
	});

// Let's put a sequence number aside so we can use it in the event callbacks
var seq = 0,
delays = 40,
durations = 200;

// Once the chart is fully created we reset the sequence
chart.on('created', function() {
	seq = 0;
});

// On each drawn element by Chartist we use the Chartist.Svg API to trigger SMIL animations
chart.on('draw', function(data) {
	seq++;

	if(data.type === 'line') {
    // If the drawn element is a line we do a simple opacity fade in. This could also be achieved using CSS3 animations.
    data.element.animate({
    	opacity: {
        // The delay when we like to start the animation
        begin: seq * delays + 1000,
        // Duration of the animation
        dur: durations,
        // The value where the animation should start
        from: 0,
        // The value where it should end
        to: 1
    }
});
} else if(data.type === 'label' && data.axis === 'x') {
	data.element.animate({
		y: {
			begin: seq * delays,
			dur: durations,
			from: data.y + 100,
			to: data.y,
        // We can specify an easing function from Chartist.Svg.Easing
        easing: 'easeOutQuart'
    }
});
} else if(data.type === 'label' && data.axis === 'y') {
	data.element.animate({
		x: {
			begin: seq * delays,
			dur: durations,
			from: data.x - 100,
			to: data.x,
			easing: 'easeOutQuart'
		}
	});
} else if(data.type === 'point') {
	data.element.animate({
		x1: {
			begin: seq * delays,
			dur: durations,
			from: data.x - 10,
			to: data.x,
			easing: 'easeOutQuart'
		},
		x2: {
			begin: seq * delays,
			dur: durations,
			from: data.x - 10,
			to: data.x,
			easing: 'easeOutQuart'
		},
		opacity: {
			begin: seq * delays,
			dur: durations,
			from: 0,
			to: 1,
			easing: 'easeOutQuart'
		}
	});
} else if(data.type === 'grid') {
    // Using data.axis we get x or y which we can use to construct our animation definition objects
    var pos1Animation = {
    	begin: seq * delays,
    	dur: durations,
    	from: data[data.axis + '1'] - 30,
    	to: data[data.axis + '1'],
    	easing: 'easeOutQuart'
    };

    var pos2Animation = {
    	begin: seq * delays,
    	dur: durations,
    	from: data[data.axis + '2'] - 100,
    	to: data[data.axis + '2'],
    	easing: 'easeOutQuart'
    };

    var animations = {};
    animations[data.axis + '1'] = pos1Animation;
    animations[data.axis + '2'] = pos2Animation;
    animations['opacity'] = {
    	begin: seq * delays,
    	dur: durations,
    	from: 0,
    	to: 1,
    	easing: 'easeOutQuart'
    };

    data.element.animate(animations);
}
});

// For the sake of the example we update the chart every time it's created with a delay of 10 seconds
chart.on('created', function() {
	if(window.__exampleAnimateTimeout) {
		clearTimeout(window.__exampleAnimateTimeout);
		window.__exampleAnimateTimeout = null;
	}
	window.__exampleAnimateTimeout = setTimeout(chart.update.bind(chart), 102000);
});


// second chart


new Chartist.Bar('#small_bar_chart', {
  labels: ['jan', 'Feb', 'Mar', 'Aprl','June','July','Aug', 'Oct'],
  series: [
    [800000, 1200000, 1400000, 1300000, 1000000,1300000,1300000],
    [200000, 400000, 500000, 300000, 1000000,1300000,1300000],
    [100000, 200000, 400000, 600000, 1000000,1300000,1300000]
  ]
}, {
  stackBars: true,
  axisY: {
    labelInterpolationFnc: function(value) {
      return (value / 1000) + 'k';
    }
  }
}).on('draw', function(data) {
  if(data.type === 'bar') {
    data.element.attr({
      style: 'stroke-width: 6px'
    });    
  }
});



// bar_chart
// var chart = new Chartist.Bar('#small_chart', {
//   labels: [1, 2, 3, 4, 5],
//   series: [[1, 2, 3, 4], [2, 3, 5, 2]]
// });

// chart.on('draw', function(data) {
//   if(data.type === 'bar') {
//     data.element.animate({
//       y2: {
//         dur: 1000,
//         from: data.y1,
//         to: data.y2,
//         easing: Chartist.Svg.Easing.easeOutQuint
//       },
//       opacity: {
//         dur: 1000,
//         from: 0,
//         to: 1,
//         easing: Chartist.Svg.Easing.easeOutQuint
//       }
//     });
//   }
// });


// sine chart
var data = {
    series: [0, 0, 4]
};

var sum = function (a, b) { return a + b };

new Chartist.Pie('#small_pie_chart', data, {
    labelInterpolationFnc: function (value) {
        return Math.round(value / data.series.reduce(sum) * 100) + '%';
    }
});






