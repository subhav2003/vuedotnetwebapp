// src/store/useCart.js
import { reactive, computed } from 'vue'
import axios from 'axios'

// Base URL from your .env
const apiBaseUrl = process.env.VUE_APP_API_URL || 'http://localhost:8000'

// auto-attach Bearer token
axios.interceptors.request.use(config => {
    const token = localStorage.getItem('authToken')
    if (token) config.headers.Authorization = `Bearer ${token}`
    return config
})

// reactive state
const state = reactive({
    cartItems: [],
    isAuthenticated: false
})

// check for token
function checkAuthentication() {
    state.isAuthenticated = !!localStorage.getItem('authToken')
}

// fetch cart from API and map bookId → product.id
async function fetchCart() {
    if (!state.isAuthenticated) {
        state.cartItems = []
        return
    }

    try {
        const { data } = await axios.get(`${apiBaseUrl}/api/Cart`)
        const productsArray = data.cart?.products ?? []
        state.cartItems = productsArray.map(p => ({
            product: {
                // copy all returned fields…
                ...p,
                // alias bookId → id so updateQuantity can find it
                id: p.bookId,
                images: Array.isArray(p.images) && p.images.length
                    ? p.images
                    : ['/images/default-book.jpg']
            },
            quantity: p.quantity
        }))
    } catch (err) {
        console.error('useCart: fetchCart failed', err)
        state.cartItems = []
    }
}

// Public entry to initialize auth + cart on app start
export function checkState() {
    checkAuthentication()
    // fire-and-forget
    fetchCart().catch(err => {
        console.error('useCart: checkState fetchCart error', err)
    })
}

// Add an item to the cart
async function addToCart(product, quantity = 1) {
    if (!state.isAuthenticated) {
        return { success: false, message: 'Please log in to purchase products.' }
    }
    try {
        const { data } = await axios.post(`${apiBaseUrl}/api/Cart/add`, {
            productId: product.id,
            quantity
        })
        // re-sync entire cart
        await fetchCart()
        return { success: true, message: data.message || 'Item added to cart.' }
    } catch (err) {
        const msg = err.response?.data?.message || 'Failed to add item'
        return { success: false, message: msg }
    }
}

// Update quantity of a cart item
async function updateQuantity(productId, quantity) {
    if (!state.isAuthenticated) {
        return { success: false, message: 'Please log in to modify the cart.' }
    }
    try {
        const { data } = await axios.put(`${apiBaseUrl}/api/Cart/update/${productId}`, {
            quantity
        })
        await fetchCart()
        return { success: true, message: data.message || 'Quantity updated.' }
    } catch (err) {
        const msg = err.response?.data?.message || 'Failed to update quantity'
        return { success: false, message: msg }
    }
}

// Remove a single item
async function removeFromCart(productId) {
    if (!state.isAuthenticated) {
        return { success: false, message: 'Please log in to modify the cart.' }
    }
    try {
        const { data } = await axios.delete(`${apiBaseUrl}/api/Cart/remove/${productId}`)
        await fetchCart()
        return { success: true, message: data.message || 'Item removed.' }
    } catch (err) {
        const msg = err.response?.data?.message || 'Failed to remove item'
        return { success: false, message: msg }
    }
}

// Clear the entire cart
async function clearCart() {
    if (!state.isAuthenticated) {
        return { success: false, message: 'Please log in to modify the cart.' }
    }
    try {
        const { data } = await axios.delete(`${apiBaseUrl}/api/Cart/clear`)
        await fetchCart()
        return { success: true, message: data.message || 'Cart cleared.' }
    } catch (err) {
        const msg = err.response?.data?.message || 'Failed to clear cart'
        return { success: false, message: msg }
    }
}

// computed getters
const totalItems = computed(() =>
    state.cartItems.reduce((sum, i) => sum + i.quantity, 0)
)
const totalAmount = computed(() =>
    state.cartItems.reduce((sum, i) => sum + i.product.price * i.quantity, 0)
)

// Composition API “hook” for components
export function useCart() {
    return {
        // reactive state
        cartItems: computed(() => state.cartItems),
        isAuthenticated: computed(() => state.isAuthenticated),

        // actions
        addToCart,
        updateQuantity,
        removeFromCart,
        clearCart,

        // getters
        totalItems,
        totalAmount
    }
}
