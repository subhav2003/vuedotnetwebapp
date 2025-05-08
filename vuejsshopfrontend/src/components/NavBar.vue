<template>
  <nav class="bg-black py-4 shadow-md sticky top-0 z-10 text-[#C8B280]">
    <!-- Toasts -->
    <Toast :toasts="toasts" @close="removeToast" />

    <div class="flex justify-between items-center container mx-auto px-4">
      <!-- Logo -->
      <router-link to="/" class="inline-block">
        <img src="@/assets/pustakalaya-logo.png" alt="Pustakalaya Logo"
             class="w-12 h-12 md:w-14 md:h-14" />
      </router-link>

      <!-- Center Links -->
      <ul class="hidden md:flex space-x-6">
        <li>
          <router-link to="/" class="text-lg font-semibold hover:text-[#A47148]"
                       exact-active-class="text-white font-bold">Home</router-link>
        </li>
<!--        <li>
          <router-link to="/connect" class="text-lg font-semibold hover:text-[#A47148]"
                       exact-active-class="text-white font-bold">Connect</router-link>
        </li>-->
        <li>
          <router-link to="/shop" class="text-lg font-semibold hover:text-[#A47148]"
                       exact-active-class="text-white font-bold">Shop</router-link>
        </li>
        <li v-if="!isLoggedIn">
          <router-link to="/auth" class="text-lg font-semibold hover:text-[#A47148]"
                       exact-active-class="text-white font-bold">Login</router-link>
        </li>
      </ul>

      <!-- Right Icons -->
      <div class="flex items-center space-x-6 relative">
        <!-- Notifications / Messages -->
        <div v-if="isLoggedIn" class="relative">
          <button @click="toggleNotificationDropdown"
                  class="relative hover:text-[#FFD700]">
            <font-awesome-icon :icon="['fas','bell']" class="text-2xl" />
            <span v-if="notificationCount > 0"
                  class="absolute top-0 right-0 bg-red-500 text-white text-xs rounded-full px-1">
              {{ notificationCount }}
            </span>
          </button>

          <div v-if="showNotificationDropdown"
               class="absolute right-0 mt-2 w-72 bg-[#121212] text-[#C8B280] shadow-lg rounded-lg z-50">
            <div class="flex border-b border-[#3d3d3d]">
              <button @click="activeTab='notifications'"
                      :class="['flex-1 px-4 py-2 text-sm',
                               activeTab==='notifications' ? 'bg-[#1c1c1c] font-bold' : '']">
                Notifications
              </button>
              <button @click="activeTab='messages'"
                      :class="['flex-1 px-4 py-2 text-sm',
                               activeTab==='messages' ? 'bg-[#1c1c1c] font-bold' : '']">
                Messages
              </button>
            </div>
            <div class="p-3 text-sm max-h-64 overflow-y-auto">
              <!-- Notifications Tab -->
              <div v-if="activeTab==='notifications'">
                <div v-for="n in notifications" :key="n.id"
                     class="flex items-start border border-[#3a3a3a] rounded p-2 mb-2 bg-[#2a200f]">
                  <span v-if="n.read===0"
                        @click.stop="markNotificationAsRead(n)"
                        class="w-2 h-2 bg-red-500 rounded-full mr-2 mt-1 cursor-pointer"
                        title="Mark as read"></span>
                  <p class="flex-1">{{ n.message }}</p>
                </div>
                <p v-if="notifications.length===0" class="text-gray-400">
                  No new notifications.
                </p>
              </div>

              <!-- Messages Tab -->
              <div v-else>
                <div v-for="o in acceptedOrders" :key="o.id"
                     @click="openChat(o.id)"
                     :class="['border rounded p-2 mb-2 cursor-pointer transition',
                              unreadOrderIds.includes(o.id)
                              ? 'border-blue-400 bg-[#1c1c2c] ring-2 ring-blue-300'
                              : 'border-[#3a3a3a] hover:bg-[#1e1e1e]']">
                  <p class="font-semibold text-sm">Order #{{ o.id }}</p>
                  <p class="text-xs text-gray-400 truncate">
                    {{ o.description || 'No description' }}
                  </p>
                </div>
                <p v-if="acceptedOrders.length === 0" class="text-gray-400 text-sm">
                  No accepted custom orders yet.
                </p>
              </div>
            </div>
          </div>
        </div>

        <!-- Cart Icon -->
        <CartIcon v-if="isLoggedIn" />

        <!-- Profile Dropdown -->
        <div v-if="isLoggedIn" class="relative">
          <button @click="toggleProfileDropdown" class="hover:text-[#FFD700]">
            <font-awesome-icon :icon="['fas','user']" class="text-2xl" />
          </button>
          <div v-if="showProfileDropdown"
               class="absolute right-0 mt-2 w-40 bg-[#121212] text-[#C8B280] shadow-lg rounded-lg py-2 z-50">
            <router-link to="/profile"
                         class="block px-4 py-2 text-sm hover:bg-[#1c1c1c]">Profile</router-link>
            <button @click="logout"
                    class="block w-full text-left px-4 py-2 text-sm hover:bg-[#1c1c1c]">
              Logout
            </button>
          </div>
        </div>

        <!-- Mobile Menu -->
        <div class="md:hidden relative">
          <button @click="isMobileOpen = !isMobileOpen" class="text-[#C8B280]">
            <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                    d="M4 6h16M4 12h16M4 18h16"/>
            </svg>
          </button>
          <ul v-if="isMobileOpen"
              class="absolute right-0 mt-2 w-48 bg-[#121212] text-[#C8B280] shadow-lg rounded-lg py-2 z-50">
            <li><router-link to="/" class="block px-4 py-2 text-sm hover:bg-[#1c1c1c]">Home</router-link></li>
            <li><router-link to="/connect" class="block px-4 py-2 text-sm hover:bg-[#1c1c1c]">Connect</router-link></li>
            <li><router-link to="/shop" class="block px-4 py-2 text-sm hover:bg-[#1c1c1c]">Shop</router-link></li>
            <li v-if="!isLoggedIn"><router-link to="/auth" class="block px-4 py-2 text-sm hover:bg-[#1c1c1c]">Login</router-link></li>
          </ul>
        </div>
      </div>
    </div>

    <!-- Chat Overlay -->
    <div v-if="activeChatOrderId"
         class="fixed inset-0 bg-black bg-opacity-50 z-50 flex justify-center items-center">
      <div class="bg-[#181818] w-full max-w-2xl h-[90vh] rounded-lg shadow-lg overflow-hidden relative">
        <button @click="activeChatOrderId = null"
                class="absolute top-2 right-2 text-xl text-[#C8B280] hover:text-red-500 z-10">
          &times;
        </button>
        <ChatComponent :order-id="activeChatOrderId" />
      </div>
    </div>
  </nav>
</template>


<script setup>
import { ref, reactive, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import Pusher from 'pusher-js'
import CartIcon from '@/components/CartIcon.vue'
import ChatComponent from '@/components/ChatComponent.vue'
import Toast from '@/components/Toast.vue'
import { checkState } from '@/store/useCart'

// — state —
const isLoggedIn              = ref(false)
const showProfileDropdown     = ref(false)
const showNotificationDropdown = ref(false)
const isMobileOpen            = ref(false)
const notificationCount       = ref(0)
const activeTab               = ref('notifications')
const acceptedOrders          = ref([])
const notifications           = ref([])
const unreadOrderIds          = ref([])
const activeChatOrderId       = ref(null)

// — toast stack —
const toasts = reactive([])

function showToast(message, type='info') {
  const id = Date.now() + Math.random()
  toasts.push({ id, message, type })
  setTimeout(()=> removeToast(id), 4000)
}
function removeToast(id) {
  const i = toasts.findIndex(t=>t.id===id)
  if(i!==-1) toasts.splice(i,1)
}

// — router & env —
const router          = useRouter()
const API_BASE        = process.env.VUE_APP_API_URL
const PUSHER_KEY      = process.env.VUE_APP_PUSHER_KEY || 'f3e9ed6993f9c51b233c'
const PUSHER_CLUSTER  = process.env.VUE_APP_PUSHER_CLUSTER || 'ap2'

// — auth header —
const authHeader = () => {
  const t = localStorage.getItem('authToken')
  return t ? { Authorization:`Bearer ${t}` } : {}
}

// — fetch accepted orders + subscribe to chat events —
async function fetchAcceptedOrders() {
  try {
    const res = await axios.get(`${API_BASE}/api/v1/accepted-custom-orders`, {
      headers: authHeader()
    })
    acceptedOrders.value = res.data.data || []

    const userId = +localStorage.getItem('userId')
    const pusher = new Pusher(PUSHER_KEY, {
      cluster: PUSHER_CLUSTER,
      forceTLS: true
    })

    acceptedOrders.value.forEach(o => {
      const ch = pusher.subscribe(`order.${o.id}`)
      ch.bind('message.sent', () => {
        // generic toast on new message
        showToast('New message received', 'success')

        // update badge
        notificationCount.value++
        if (!unreadOrderIds.value.includes(o.id)) {
          unreadOrderIds.value.push(o.id)
        }
      })
    })
  } catch(e){ console.error(e) }
}

// — fetch unread notifications —
async function fetchNotifications() {
  try {
    const res = await axios.get(`${API_BASE}/api/v1/notifications`, {
      headers: authHeader()
    })
    const all = res.data.data || []
    notifications.value = all.filter(n=>n.read===0)
    notificationCount.value = notifications.value.length
  } catch(e){ console.error(e) }
}

// — mark one notification as read —
async function markNotificationAsRead(n) {
  try {
    await axios.post(`${API_BASE}/api/v1/notifications/${n.id}/read`, {}, {
      headers: authHeader()
    })
    n.read = 1
    notifications.value = notifications.value.filter(x=>x.read===0)
    notificationCount.value = notifications.value.length
    showToast('Notification marked as read', 'success')
  } catch(e){ console.error(e) }
}

// — open chat pane —
function openChat(id) {
  activeChatOrderId.value = id
  notificationCount.value = 0
  unreadOrderIds.value = unreadOrderIds.value.filter(x=>x!==id)
}

// — server‑side notifications via Pusher —
function initPusherNotifications() {
  const userId = +localStorage.getItem('userId')
  const pusher = new Pusher(PUSHER_KEY, {
    cluster: PUSHER_CLUSTER, forceTLS: true
  })
  const ch = pusher.subscribe(`user.notifications.${userId}`)
  ch.bind('notification', data => {
    notifications.value.unshift({ ...data, read:0 })
    notificationCount.value++
    showToast(data.message, 'info')
  })
}

// — toggles & logout —
function toggleProfileDropdown() {
  showProfileDropdown.value = !showProfileDropdown.value
  showNotificationDropdown.value = false
}
function toggleNotificationDropdown() {
  showNotificationDropdown.value = !showNotificationDropdown.value
  if (showNotificationDropdown.value) {
    notificationCount.value = 0
  }
}
async function logout() {
  try {
    await axios.post(`${API_BASE}/api/v1/logout`, {}, {
      headers: authHeader()
    })
  } catch {}
  localStorage.clear()
  isLoggedIn.value = false
  router.push('/auth')
  window.location.reload()
}

// — wire up on mount —
onMounted(() => {
  isLoggedIn.value = !!localStorage.getItem('authToken')
  if (isLoggedIn.value) {
    fetchNotifications()
    fetchAcceptedOrders()
    initPusherNotifications()
  }
  checkState()
})

// — prevent scroll when chat open —
watch(activeChatOrderId, val => {
  document.body.style.overflow = val ? 'hidden' : 'auto'
})
</script>

<style scoped>
.nav-link { font-size: 1.125rem; font-weight:600; font-family:Papyrus, fantasy; color:#73442f; }
.nav-link:hover { color:#975c3d; }
.nav-link-active { color:#472c20; font-weight:700; }
.mobile-link { display:block; padding:.5rem 1rem; font-size:.875rem; color:#73442f; }
.mobile-link:hover { background-color:#f3f0e7; color:#472c20; }

</style>
