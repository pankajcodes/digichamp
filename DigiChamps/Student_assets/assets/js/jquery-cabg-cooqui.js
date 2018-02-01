/**
 *	jQuery Canvas Animated Background: Cooqui
 *	Copyright (c) 2016 Gonzalo Albito Méndez Rey
 *	Licensed under GNU GPL 3.0 (https://www.gnu.org/licenses/gpl-3.0-standalone.html)
 *	@version	0.8.1	(2016-07-18)
 *	@author		Gonzalo Albito Méndez Rey	<gonzalo@albito.es>
 *	@license	GPL-3.0
 */

(function($){
	$.fn.extend({
		cooqui: function(options){
	    	var defaults = {
					fps: 60,
					scale: 1,
					distance: 100,
					background: false,
					spacing: 50,
					itemSize: 50,
					itemScale: 2.0,
					itemColors: ["#0000ff", "#00ff00", "#00ffff", "#ff0000", "#ff00ff", "#ffff00"],
					unique: true,
				};
	    	
        	options = $.extend({}, defaults, options);
        	
        	return this.each(function(){
				var element = $(this);
				new Cooqui(options, element);
			});
		}
	});
	
	function Cooqui(opts, elemt)
	{
		var obj = this;
		var element = elemt;
		var options = opts;
		var jCanvas = null;
		var canvas = null;
		var context = null;
		var frameTime = Math.floor(1000/options.fps);
		var pi2 = 2*Math.PI;
		var spacing = options.spacing+options.itemSize;
		var mouse = {
						x: -1,
						y: -1,
						xMin: -1,
						xMax: -1,
						yMin: -1,
						yMax: -1
					};
		
		var random = function(min, max){
				var diff = max-min;
				return Math.floor(Math.random()*diff)+min;
			};
		
		var clear = function(){
				context.clearRect(0, 0, canvas.width, canvas.height);
				if(options.background)
				{
					context.fillStyle = options.background;
					context.fillRect(0, 0, canvas.width, canvas.height);
				}
			};
		
		var drawCircle = function(x, y, radius, color){
				context.beginPath();
				context.arc(x, y, radius, 0, pi2, false);
				if(color)
				{
					context.fillStyle = color;
					context.fill();
				}
			};
		
		var draw = function(){
				clear();
				var iMax = (canvas.width/spacing)+1;
				var jMax = (canvas.height/spacing)+1;
				var c = 0;
				for(var i=0; i<iMax; i++)
				{
					var x = i*spacing;
					for(var j=0; j<jMax; j++)
					{
						if((i+j)%2==0)
						{
							var y = j*spacing;
							var radius = options.itemSize;
							if(mouse.x>0 && mouse.y>0 && x>mouse.xMin && x<mouse.xMax && y>mouse.yMin && y<mouse.yMax)
							{
								var distance = Math.floor(Math.sqrt(Math.pow(x-mouse.x, 2)+Math.pow(y-mouse.y, 2)));
								if(distance<options.distance)
								{
									var scale = (options.distance-distance)/options.distance;
									scale += 1;
									radius = Math.floor(radius*scale);
								}
							}
							var color = options.itemColors[c++%options.itemColors.length];
							drawCircle(x, y, radius, color);
						}
						
					}
				}
				context.globalAlpha = 1;
			};
		
		var update = function(time){
				//NOTHING
			};
		
		var loop = function(){
				update(frameTime);
				draw();
				setTimeout(loop, frameTime);
			};
		
		var resize = function(){
				canvas.width = Math.floor(jCanvas.width()/options.scale);
				canvas.height = Math.floor(jCanvas.height()/options.scale);
			};
		
		var init = function(){
				if(options.unique)
				{
					element.children(".jq-cabg-canvas").remove();
				}
				jCanvas = $("<canvas class=\"jq-cabg-canvas interactive-background\"></canvas>");
				jCanvas.css({position:"fixed",left:"0px",top:"0px",right:"0px",bottom:"0px",width:"100%",height:"100%",zIndex:"-1"});
				element.addClass("jq-cabg canvas-background");
				element.append(jCanvas);
				canvas = jCanvas[0];
				context = canvas.getContext("2d");
				var win = $(window);
				win.resize(resize);
				var doc = $(document);
				doc.mouseout(function(ev){
						mouse.x = -1;
						mouse.y = -1;
						//draw();
					});
				doc.mousemove(function(ev){
						mouse.x = ev.clientX;
						mouse.y = ev.clientY;
						mouse.xMin = mouse.x-options.distance;
						mouse.xMax = mouse.x+options.distance;
						mouse.yMin = mouse.y-options.distance;
						mouse.yMax = mouse.y+options.distance;
						//draw();
					});
				resize();
				loop();
				//draw();
			};
		
		init();
	};
})(jQuery);