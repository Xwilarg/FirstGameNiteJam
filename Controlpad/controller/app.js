let global_spec;

// -------- GameNite Controller App --------

// ---- Globals ----

var messages = [];

// ---- Messages ----


// Specify the list of messages to be sent to the console
function outgoingMessages() {
    temp = messages;
    messages = [];
    return temp;
}

// ---- Touch Handlers ----

// handleClick should only be used for testing in PC browser
function handleClick(x, y) {
    handleTouchStart(0, x, y);
}

function getTouchId(x, y, callback) {
    global_spec.buttons.forEach(function (b, i) {
        if (x >= b.x && x <= b.x + b.w &&
        y >= b.y && y <= b.y + b.h)
        {
            b.depressed = true;
            callback(b.id);
        }
    });
}

// Handle a single touch as it starts
function handleTouchStart(id, x, y) {
    let msg = "TouchStart(" + x.toString() + "," + y.toString() + ")";
    getTouchId(x, y, (bid) => {
        messages.push(`{${bid}};1`);
    });
}

// Handle a single touch that has moved
function handleTouchMove(id, x, y) {
    //let msg = "TouchMove(" + x.toString() + "," + y.toString() + ")";
    //messages.push(msg);
}

// Handle a single touch that has ended
function handleTouchEnd(id, x, y) {
    let msg = "TouchEnd(" + x.toString() + "," + y.toString() + ")";
    getTouchId(x, y, (bid) => {
        messages.push(`{${bid}};0`);
    });
}

// Handle a single touch that has ended in an unexpected way
function handleTouchCancel(id, x, y) {
    //let msg = "TouchCancel(" + x.toString() + "," + y.toString() + ")";
    //messages.push(msg);
}

// ---- Start and Update ----

// Called once upon page load (load your resources here)
function controlpadStart(width, height) {
}

// Called 30 times per second
function controlpadUpdate() {
}


// Inspired from https://github.com/RecBox-Games/controlpad_test_server/blob/main/controller/index.html
function drawController(canvas, ctx) {
    const yellow = 'rgba(220, 200, 70, .9)';
    const faded_yellow = 'rgba(220, 200, 70, .2)';
    const red = 'rgba(230, 120, 70, 1)';
    const green = 'rgba(120, 220, 100, 1)';

    
    const w = canvas.width;
    const h = canvas.height;
    const aw = w*.45;
    const bw = w - aw;
    // dpad
    const thic1 = h*.125;
    const thic2 = h*.15;
    const dpad_x = aw/2;
    const dpad_y = h/2;
    const up_x = dpad_x-thic1/2;
    const up_y = dpad_y-thic1*.8-thic2;
    const down_x = dpad_x-thic1/2;
    const down_y = dpad_y+thic1*.8;
    const left_x = dpad_x-thic1*.8-thic2;
    const left_y = dpad_y-thic1/2;
    const right_x = dpad_x+thic1*.8;
    const right_y = dpad_y-thic1/2;
    const sq1 = bw*.45;
    const action_x = w-sq1-bw/12;
    const action_y = (h-sq1)/2;

    global_spec = {
        'buttons': [
            {'x':up_x,    'y':up_y,   'w':thic1, 'h':thic2, 'id':'up'},
            {'x':down_x,  'y':down_y, 'w':thic1, 'h':thic2, 'id':'down'},
            {'x':left_x,  'y':left_y, 'w':thic2, 'h':thic1, 'id':'left'},
            {'x':right_x, 'y':right_y,'w':thic2, 'h':thic1, 'id':'right'},
            {'x':action_x, 'y':action_y,'w':sq1, 'h':sq1,   'id':'action'}
        ],
        'panels': [
            {'x': dpad_x-thic1/2, 'y':dpad_y-thic1/2, 'w':thic1, 'h':thic1, 'color':faded_yellow},
            {'x': up_x-10, 'y':up_y-10, 'w':thic1+20, 'h':thic2+10, 'color':yellow},
            {'x': down_x-10, 'y':down_y, 'w':thic1+20, 'h':thic2+10, 'color':yellow},
            {'x': left_x-10, 'y':left_y-10, 'w':thic2+10, 'h':thic1+20, 'color':yellow},
            {'x': right_x, 'y':right_y-10, 'w':thic2+10, 'h':thic1+20, 'color':yellow},
            {'x': action_x-10, 'y':action_y-10,'w':sq1+20, 'h':sq1+20,   'color':red}
            
        ]
    }
    
    ctx.beginPath();
    ctx.fillStyle = 'rgb(245,245,245)';
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // Draw panels
    global_spec.panels.forEach(function (p, i) {
        ctx.beginPath();
        ctx.fillStyle = p.color;
        ctx.fillRect(p.x, p.y, p.w, p.h);
    });
    // Draw buttons
    global_spec.buttons.forEach(function (b, i) {
        ctx.beginPath();
        if (b.depressed) {
        ctx.fillStyle = 'rgb(190,190,190)';
        } else {
        ctx.fillStyle = 'rgb(230,230,230)';
        }
        ctx.rect(b.x, b.y, b.w, b.h, 10);
        ctx.stroke();
        ctx.fill();
    });
}