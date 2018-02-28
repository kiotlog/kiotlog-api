/// <reference path="../bower_components/polymer/types/lib/utils/render-status.d.ts" />

namespace Kiotlog {
    const {customElement, property, observe, query, computed} = Polymer.decorators;

    // Gesture events like tap and track generated from touch will not be
    // preventable, allowing for better scrolling performance.
    Polymer.setPassiveTouchGestures(true);

    @customElement('ktlg-app')
    export class KtlgApp extends Polymer.Element {
        // static get is() { return 'ktlg-app'; }

        @property({type: String, reflectToAttribute: true, observer: KtlgApp.prototype._pageChanged})
        page: string;

        @property({type: Object})
        routeData: object;

        @property({type: Object})
        subroute: object;

        // This shouldn't be neccessary, but the Analyzer isn't picking up
        // Polymer.Element#rootPath
        @property({type: String})
        rootPath: string;

        @property({type: String})
        apiUrl: string = 'http://127.0.0.1:8888/';

        // static get properties() {
        //     return {
        //         page: {
        //             type: String,
        //             reflectToAttribute: true,
        //             observer: '_pageChanged',
        //         },
        //         routeData: Object,
        //         subroute: Object,
        //         // This shouldn't be neccessary, but the Analyzer isn't picking up
        //         // Polymer.Element#rootPath
        //         rootPath: String,
        //     };
        // }

        // static get observers() {
        //     return [
        //         '_routePageChanged(routeData.page)',
        //     ];
        // }

        @query('#drawer')
        drawer: any; // TODO: Add type when available

        @observe('routeData.page')
        _routePageChanged(page: string) {
            // If no page was found in the route data, page will be an empty string.
            // Default to 'view1' in that case.
            this.page = page || 'dashboard';

            // Close a non-persistent drawer when the page & route are changed.
            if (!(this.drawer).persistent) {
                this.drawer.close();
            }
        }

        _pageChanged(page: String) {
            // Load page import on demand. Show 404 page if fails
            const resolvedPageUrl = this.resolveUrl('ktlg-' + page + '.html');
            Polymer.importHref(
                resolvedPageUrl,
                null,
                this._showPage404.bind(this),
                true);
        }

        _showPage404() {
            this.page = 'view404';
        }
    }
}