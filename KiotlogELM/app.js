import mdcAutoInit from '@material/auto-init';
import { MDCTopAppBar } from '@material/top-app-bar/index';
import { MDCTemporaryDrawer } from '@material/drawer';
import { MDCTextField } from '@material/textfield';
import { MDCSelect } from '@material/select';
import { MDCRipple } from '@material/ripple';

function setupMDCObserver(node) {
    let observer = new MutationObserver(function (mutations) {
        // keep track of target nodes already processed for this mutation record list.
        let processedTargets = [];
        let ntargets = 0;
        const nm = mutations.length;

        for (let i = 0; i < nm; i++) {
            let mutation = mutations[i];
            if (mutation.addedNodes.length > 0) {
                let alreadyProcessed = false;
                for (let j = 0; j < ntargets; j++) {
                    if (processedTargets[j] === mutation.target) {
                        alreadyProcessed = true;
                        break;
                    }
                }
                if (!alreadyProcessed) {
                    mdcAutoInit(mutation.target, () => { });
                    processedTargets.push(mutation.target);
                    ntargets++;
                }
            }
        }
    });

    let config = { childList: true, subtree: true };
    observer.observe(node, config);
    return observer;
}

mdcAutoInit.register('MDCTextField', MDCTextField);
mdcAutoInit.register('MDCSelect', MDCSelect);
mdcAutoInit.register('MDCRipple', MDCRipple);

const mdcObserver = setupMDCObserver(document.getElementById('kiotlog-app'));
const app = Elm.Main.embed(document.getElementById("kiotlog-app"));


var drawer;
var toolbar

app.ports.initMDC.subscribe(function () {
    // window.mdc.autoInit();
    // mdcAutoInit();
    drawer = new MDCTemporaryDrawer(document.getElementById('kiotlog-actions-menu'));
    toolbar = new MDCTopAppBar(document.getElementById('kiotlog-top-app-bar'));
});

app.ports.openDrawer.subscribe(function () {
    // document.getElementById('kiotlog-actions-menu').MDCTemporaryDrawer.open = true;
    drawer.open = true;
});

app.ports.closeDrawer.subscribe(function () {
    // document.getElementById('kiotlog-actions-menu').MDCTemporaryDrawer.open = false;
    drawer.open = false;
});

// app.ports.initMDCFields.subscribe(function() {
//     // document.getElementById('kiotlog-actions-menu').MDCTemporaryDrawer.open = true;
//     // const elmts = document.querySelector('.mdc-text-field');
//     // console.log(elmts);
//     console.log('text');

//     const textFieldElements = [].slice.call(document.querySelectorAll('.mdc-text-field'));
//     textFieldElements.forEach((textFieldEl) => {
//     console.log(textFieldEl);

//         new MDCTextField(textFieldEl);
//     });
//     // const textField = new MDCTextField();
// });
